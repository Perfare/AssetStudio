#include "etc.h"
#include <stdint.h>
#include <string.h>
#include "color.h"

const uint_fast8_t WriteOrderTable[16] = {0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15};
const uint_fast8_t WriteOrderTableRev[16] = {15, 11, 7, 3, 14, 10, 6, 2, 13, 9, 5, 1, 12, 8, 4, 0};
const uint_fast8_t Etc1ModifierTable[8][2] = {{2, 8},   {5, 17},  {9, 29},   {13, 42},
                                              {18, 60}, {24, 80}, {33, 106}, {47, 183}};
const uint_fast8_t Etc2aModifierTable[2][8][2] = {
  {{0, 8}, {0, 17}, {0, 29}, {0, 42}, {0, 60}, {0, 80}, {0, 106}, {0, 183}},
  {{2, 8}, {5, 17}, {9, 29}, {13, 42}, {18, 60}, {24, 80}, {33, 106}, {47, 183}}};
const uint_fast8_t Etc1SubblockTable[2][16] = {{0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1},
                                               {0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1}};
const uint_fast8_t Etc2DistanceTable[8] = {3, 6, 11, 16, 23, 32, 41, 64};
const int_fast8_t Etc2AlphaModTable[16][8] = {
  {-3, -6, -9, -15, 2, 5, 8, 14}, {-3, -7, -10, -13, 2, 6, 9, 12}, {-2, -5, -8, -13, 1, 4, 7, 12},
  {-2, -4, -6, -13, 1, 3, 5, 12}, {-3, -6, -8, -12, 2, 5, 7, 11},  {-3, -7, -9, -11, 2, 6, 8, 10},
  {-4, -7, -8, -11, 3, 6, 7, 10}, {-3, -5, -8, -11, 2, 4, 7, 10},  {-2, -6, -8, -10, 1, 5, 7, 9},
  {-2, -5, -8, -10, 1, 4, 7, 9},  {-2, -4, -8, -10, 1, 3, 7, 9},   {-2, -5, -7, -10, 1, 4, 6, 9},
  {-3, -4, -7, -10, 2, 3, 6, 9},  {-1, -2, -3, -10, 0, 1, 2, 9},   {-4, -6, -8, -9, 3, 5, 7, 8},
  {-3, -5, -7, -9, 2, 4, 6, 8}};

static inline uint_fast8_t clamp(const int n) {
    return n < 0 ? 0 : n > 255 ? 255 : n;
}

static inline uint32_t applicate_color(uint_fast8_t c[3], int_fast16_t m) {
    return color(clamp(c[0] + m), clamp(c[1] + m), clamp(c[2] + m), 255);
}

static inline uint32_t applicate_color_alpha(uint_fast8_t c[3], int_fast16_t m, int transparent) {
    return color(clamp(c[0] + m), clamp(c[1] + m), clamp(c[2] + m), transparent ? 0 : 255);
}

static inline uint32_t applicate_color_raw(uint_fast8_t c[3]) {
    return color(c[0], c[1], c[2], 255);
}

static void decode_etc1_block(const uint8_t *data, uint32_t *outbuf) {
    const uint_fast8_t code[2] = {data[3] >> 5, data[3] >> 2 & 7};  // Table codewords
    const uint_fast8_t *table = Etc1SubblockTable[data[3] & 1];
    uint_fast8_t c[2][3];
    if (data[3] & 2) {
        // diff bit == 1
        c[0][0] = data[0] & 0xf8;
        c[0][1] = data[1] & 0xf8;
        c[0][2] = data[2] & 0xf8;
        c[1][0] = c[0][0] + (data[0] << 3 & 0x18) - (data[0] << 3 & 0x20);
        c[1][1] = c[0][1] + (data[1] << 3 & 0x18) - (data[1] << 3 & 0x20);
        c[1][2] = c[0][2] + (data[2] << 3 & 0x18) - (data[2] << 3 & 0x20);
        c[0][0] |= c[0][0] >> 5;
        c[0][1] |= c[0][1] >> 5;
        c[0][2] |= c[0][2] >> 5;
        c[1][0] |= c[1][0] >> 5;
        c[1][1] |= c[1][1] >> 5;
        c[1][2] |= c[1][2] >> 5;
    } else {
        // diff bit == 0
        c[0][0] = (data[0] & 0xf0) | data[0] >> 4;
        c[1][0] = (data[0] & 0x0f) | data[0] << 4;
        c[0][1] = (data[1] & 0xf0) | data[1] >> 4;
        c[1][1] = (data[1] & 0x0f) | data[1] << 4;
        c[0][2] = (data[2] & 0xf0) | data[2] >> 4;
        c[1][2] = (data[2] & 0x0f) | data[2] << 4;
    }

    uint_fast16_t j = data[6] << 8 | data[7];  // less significant pixel index bits
    uint_fast16_t k = data[4] << 8 | data[5];  // more significant pixel index bits
    for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
        uint_fast8_t s = table[i];
        uint_fast8_t m = Etc1ModifierTable[code[s]][j & 1];
        outbuf[WriteOrderTable[i]] = applicate_color(c[s], k & 1 ? -m : m);
    }
}

static void decode_etc2_block(const uint8_t *data, uint32_t *outbuf) {
    uint_fast16_t j = data[6] << 8 | data[7];  // 15 -> 0
    uint_fast32_t k = data[4] << 8 | data[5];  // 31 -> 16
    uint_fast8_t c[3][3] = {};

    if (data[3] & 2) {
        // diff bit == 1
        uint_fast8_t r = data[0] & 0xf8;
        int_fast16_t dr = (data[0] << 3 & 0x18) - (data[0] << 3 & 0x20);
        uint_fast8_t g = data[1] & 0xf8;
        int_fast16_t dg = (data[1] << 3 & 0x18) - (data[1] << 3 & 0x20);
        uint_fast8_t b = data[2] & 0xf8;
        int_fast16_t db = (data[2] << 3 & 0x18) - (data[2] << 3 & 0x20);
        if (r + dr < 0 || r + dr > 255) {
            // T
            c[0][0] = (data[0] << 3 & 0xc0) | (data[0] << 4 & 0x30) | (data[0] >> 1 & 0xc) | (data[0] & 3);
            c[0][1] = (data[1] & 0xf0) | data[1] >> 4;
            c[0][2] = (data[1] & 0x0f) | data[1] << 4;
            c[1][0] = (data[2] & 0xf0) | data[2] >> 4;
            c[1][1] = (data[2] & 0x0f) | data[2] << 4;
            c[1][2] = (data[3] & 0xf0) | data[3] >> 4;
            const uint_fast8_t d = Etc2DistanceTable[(data[3] >> 1 & 6) | (data[3] & 1)];
            uint_fast32_t color_set[4] = {applicate_color_raw(c[0]), applicate_color(c[1], d),
                                          applicate_color_raw(c[1]), applicate_color(c[1], -d)};
            k <<= 1;
            for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                outbuf[WriteOrderTable[i]] = color_set[(k & 2) | (j & 1)];
        } else if (g + dg < 0 || g + dg > 255) {
            // H
            c[0][0] = (data[0] << 1 & 0xf0) | (data[0] >> 3 & 0xf);
            c[0][1] = (data[0] << 5 & 0xe0) | (data[1] & 0x10);
            c[0][1] |= c[0][1] >> 4;
            c[0][2] = (data[1] & 8) | (data[1] << 1 & 6) | data[2] >> 7;
            c[0][2] |= c[0][2] << 4;
            c[1][0] = (data[2] << 1 & 0xf0) | (data[2] >> 3 & 0xf);
            c[1][1] = (data[2] << 5 & 0xe0) | (data[3] >> 3 & 0x10);
            c[1][1] |= c[1][1] >> 4;
            c[1][2] = (data[3] << 1 & 0xf0) | (data[3] >> 3 & 0xf);
            uint_fast8_t d = (data[3] & 4) | (data[3] << 1 & 2);
            if (c[0][0] > c[1][0] ||
                (c[0][0] == c[1][0] && (c[0][1] > c[1][1] || (c[0][1] == c[1][1] && c[0][2] >= c[1][2]))))
                ++d;
            d = Etc2DistanceTable[d];
            uint_fast32_t color_set[4] = {applicate_color(c[0], d), applicate_color(c[0], -d), applicate_color(c[1], d),
                                          applicate_color(c[1], -d)};
            k <<= 1;
            for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
                outbuf[WriteOrderTable[i]] = color_set[(k & 2) | (j & 1)];
        } else if (b + db < 0 || b + db > 255) {
            // planar
            c[0][0] = (data[0] << 1 & 0xfc) | (data[0] >> 5 & 3);
            c[0][1] = (data[0] << 7 & 0x80) | (data[1] & 0x7e) | (data[0] & 1);
            c[0][2] = (data[1] << 7 & 0x80) | (data[2] << 2 & 0x60) | (data[2] << 3 & 0x18) | (data[3] >> 5 & 4);
            c[0][2] |= c[0][2] >> 6;
            c[1][0] = (data[3] << 1 & 0xf8) | (data[3] << 2 & 4) | (data[3] >> 5 & 3);
            c[1][1] = (data[4] & 0xfe) | data[4] >> 7;
            c[1][2] = (data[4] << 7 & 0x80) | (data[5] >> 1 & 0x7c);
            c[1][2] |= c[1][2] >> 6;
            c[2][0] = (data[5] << 5 & 0xe0) | (data[6] >> 3 & 0x1c) | (data[5] >> 1 & 3);
            c[2][1] = (data[6] << 3 & 0xf8) | (data[7] >> 5 & 0x6) | (data[6] >> 4 & 1);
            c[2][2] = data[7] << 2 | (data[7] >> 4 & 3);
            for (int y = 0, i = 0; y < 4; y++) {
                for (int x = 0; x < 4; x++, i++) {
                    uint8_t r = clamp((x * (c[1][0] - c[0][0]) + y * (c[2][0] - c[0][0]) + 4 * c[0][0] + 2) >> 2);
                    uint8_t g = clamp((x * (c[1][1] - c[0][1]) + y * (c[2][1] - c[0][1]) + 4 * c[0][1] + 2) >> 2);
                    uint8_t b = clamp((x * (c[1][2] - c[0][2]) + y * (c[2][2] - c[0][2]) + 4 * c[0][2] + 2) >> 2);
                    outbuf[i] = color(r, g, b, 255);
                }
            }
        } else {
            // differential
            const uint_fast8_t code[2] = {data[3] >> 5, data[3] >> 2 & 7};
            const uint_fast8_t *table = Etc1SubblockTable[data[3] & 1];
            c[0][0] = r | r >> 5;
            c[0][1] = g | g >> 5;
            c[0][2] = b | b >> 5;
            c[1][0] = r + dr;
            c[1][1] = g + dg;
            c[1][2] = b + db;
            c[1][0] |= c[1][0] >> 5;
            c[1][1] |= c[1][1] >> 5;
            c[1][2] |= c[1][2] >> 5;
            for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
                uint_fast8_t s = table[i];
                uint_fast8_t m = Etc1ModifierTable[code[s]][j & 1];
                outbuf[WriteOrderTable[i]] = applicate_color(c[s], k & 1 ? -m : m);
            }
        }
    } else {
        // individual (diff bit == 0)
        const uint_fast8_t code[2] = {data[3] >> 5, data[3] >> 2 & 7};
        const uint_fast8_t *table = Etc1SubblockTable[data[3] & 1];
        c[0][0] = (data[0] & 0xf0) | data[0] >> 4;
        c[1][0] = (data[0] & 0x0f) | data[0] << 4;
        c[0][1] = (data[1] & 0xf0) | data[1] >> 4;
        c[1][1] = (data[1] & 0x0f) | data[1] << 4;
        c[0][2] = (data[2] & 0xf0) | data[2] >> 4;
        c[1][2] = (data[2] & 0x0f) | data[2] << 4;
        for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
            uint_fast8_t s = table[i];
            uint_fast8_t m = Etc1ModifierTable[code[s]][j & 1];
            outbuf[WriteOrderTable[i]] = applicate_color(c[s], k & 1 ? -m : m);
        }
    }
}

static void decode_etc2a1_block(const uint8_t *data, uint32_t *outbuf) {
    uint_fast16_t j = data[6] << 8 | data[7];  // 15 -> 0
    uint_fast32_t k = data[4] << 8 | data[5];  // 31 -> 16
    uint_fast8_t c[3][3] = {};

    int obaq = data[3] >> 1 & 1;

    // diff bit == 1
    uint_fast8_t r = data[0] & 0xf8;
    int_fast16_t dr = (data[0] << 3 & 0x18) - (data[0] << 3 & 0x20);
    uint_fast8_t g = data[1] & 0xf8;
    int_fast16_t dg = (data[1] << 3 & 0x18) - (data[1] << 3 & 0x20);
    uint_fast8_t b = data[2] & 0xf8;
    int_fast16_t db = (data[2] << 3 & 0x18) - (data[2] << 3 & 0x20);
    if (r + dr < 0 || r + dr > 255) {
        // T
        c[0][0] = (data[0] << 3 & 0xc0) | (data[0] << 4 & 0x30) | (data[0] >> 1 & 0xc) | (data[0] & 3);
        c[0][1] = (data[1] & 0xf0) | data[1] >> 4;
        c[0][2] = (data[1] & 0x0f) | data[1] << 4;
        c[1][0] = (data[2] & 0xf0) | data[2] >> 4;
        c[1][1] = (data[2] & 0x0f) | data[2] << 4;
        c[1][2] = (data[3] & 0xf0) | data[3] >> 4;
        const uint_fast8_t d = Etc2DistanceTable[(data[3] >> 1 & 6) | (data[3] & 1)];
        uint_fast32_t color_set[4] = {applicate_color_raw(c[0]), applicate_color(c[1], d), applicate_color_raw(c[1]),
                                      applicate_color(c[1], -d)};
        k <<= 1;
        for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
            int index = (k & 2) | (j & 1);
            outbuf[WriteOrderTable[i]] = color_set[index];
            if (!obaq && index == 2)
                outbuf[WriteOrderTable[i]] &= TRANSPARENT_MASK;
        }
    } else if (g + dg < 0 || g + dg > 255) {
        // H
        c[0][0] = (data[0] << 1 & 0xf0) | (data[0] >> 3 & 0xf);
        c[0][1] = (data[0] << 5 & 0xe0) | (data[1] & 0x10);
        c[0][1] |= c[0][1] >> 4;
        c[0][2] = (data[1] & 8) | (data[1] << 1 & 6) | data[2] >> 7;
        c[0][2] |= c[0][2] << 4;
        c[1][0] = (data[2] << 1 & 0xf0) | (data[2] >> 3 & 0xf);
        c[1][1] = (data[2] << 5 & 0xe0) | (data[3] >> 3 & 0x10);
        c[1][1] |= c[1][1] >> 4;
        c[1][2] = (data[3] << 1 & 0xf0) | (data[3] >> 3 & 0xf);
        uint_fast8_t d = (data[3] & 4) | (data[3] << 1 & 2);
        if (c[0][0] > c[1][0] ||
            (c[0][0] == c[1][0] && (c[0][1] > c[1][1] || (c[0][1] == c[1][1] && c[0][2] >= c[1][2]))))
            ++d;
        d = Etc2DistanceTable[d];
        uint_fast32_t color_set[4] = {applicate_color(c[0], d), applicate_color(c[0], -d), applicate_color(c[1], d),
                                      applicate_color(c[1], -d)};
        k <<= 1;
        for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
            int index = (k & 2) | (j & 1);
            outbuf[WriteOrderTable[i]] = color_set[index];
            if (!obaq && index == 2)
                outbuf[WriteOrderTable[i]] &= TRANSPARENT_MASK;
        }
    } else if (b + db < 0 || b + db > 255) {
        // planar
        c[0][0] = (data[0] << 1 & 0xfc) | (data[0] >> 5 & 3);
        c[0][1] = (data[0] << 7 & 0x80) | (data[1] & 0x7e) | (data[0] & 1);
        c[0][2] = (data[1] << 7 & 0x80) | (data[2] << 2 & 0x60) | (data[2] << 3 & 0x18) | (data[3] >> 5 & 4);
        c[0][2] |= c[0][2] >> 6;
        c[1][0] = (data[3] << 1 & 0xf8) | (data[3] << 2 & 4) | (data[3] >> 5 & 3);
        c[1][1] = (data[4] & 0xfe) | data[4] >> 7;
        c[1][2] = (data[4] << 7 & 0x80) | (data[5] >> 1 & 0x7c);
        c[1][2] |= c[1][2] >> 6;
        c[2][0] = (data[5] << 5 & 0xe0) | (data[6] >> 3 & 0x1c) | (data[5] >> 1 & 3);
        c[2][1] = (data[6] << 3 & 0xf8) | (data[7] >> 5 & 0x6) | (data[6] >> 4 & 1);
        c[2][2] = data[7] << 2 | (data[7] >> 4 & 3);
        for (int y = 0, i = 0; y < 4; y++) {
            for (int x = 0; x < 4; x++, i++) {
                uint8_t r = clamp((x * (c[1][0] - c[0][0]) + y * (c[2][0] - c[0][0]) + 4 * c[0][0] + 2) >> 2);
                uint8_t g = clamp((x * (c[1][1] - c[0][1]) + y * (c[2][1] - c[0][1]) + 4 * c[0][1] + 2) >> 2);
                uint8_t b = clamp((x * (c[1][2] - c[0][2]) + y * (c[2][2] - c[0][2]) + 4 * c[0][2] + 2) >> 2);
                outbuf[i] = color(r, g, b, 255);
            }
        }
    } else {
        // differential
        const uint_fast8_t code[2] = {data[3] >> 5, data[3] >> 2 & 7};
        const uint_fast8_t *table = Etc1SubblockTable[data[3] & 1];
        c[0][0] = r | r >> 5;
        c[0][1] = g | g >> 5;
        c[0][2] = b | b >> 5;
        c[1][0] = r + dr;
        c[1][1] = g + dg;
        c[1][2] = b + db;
        c[1][0] |= c[1][0] >> 5;
        c[1][1] |= c[1][1] >> 5;
        c[1][2] |= c[1][2] >> 5;
        for (int i = 0; i < 16; i++, j >>= 1, k >>= 1) {
            uint_fast8_t s = table[i];
            uint_fast8_t m = Etc2aModifierTable[obaq][code[s]][j & 1];
            outbuf[WriteOrderTable[i]] = applicate_color_alpha(c[s], k & 1 ? -m : m, !obaq && (k & 1) && !(j & 1));
        }
    }
}

static void decode_etc2a8_block(const uint8_t *data, uint32_t *outbuf) {
    if (data[1] & 0xf0) {
        // multiplier != 0
        const uint_fast8_t multiplier = data[1] >> 4;
        const int_fast8_t *table = Etc2AlphaModTable[data[1] & 0xf];
        uint_fast64_t l = bton64(*(uint64_t*)data);
        for (int i = 0; i < 16; i++, l >>= 3)
            ((uint8_t *)(outbuf + WriteOrderTableRev[i]))[3] = clamp(data[0] + multiplier * table[l & 7]);
    } else {
        // multiplier == 0 (always same as base codeword)
        for (int i = 0; i < 16; i++, outbuf++)
            ((uint8_t *)outbuf)[3] = data[0];
    }
}

static void decode_eac_block(const uint8_t *data, int color, uint32_t *outbuf) {
    uint_fast8_t multiplier = data[1] >> 1 & 0x78;
    if (multiplier == 0)
        multiplier = 1;
    const int_fast8_t *table = Etc2AlphaModTable[data[1] & 0xf];
    uint_fast64_t l = bton64(*(uint64_t*)data);
    for (int i = 0; i < 16; i++, l >>= 3) {
        int_fast16_t val = data[0] * 8 + multiplier * table[l & 7] + 4;
        ((uint8_t *)(outbuf + WriteOrderTableRev[i]))[color] = val < 0 ? 0 : val >= 2048 ? 0xff : val >> 3;
    }
}

static void decode_eac_signed_block(const uint8_t *data, int color, uint32_t *outbuf) {
    int8_t base = (int8_t)data[0];
    uint_fast8_t multiplier = data[1] >> 1 & 0x78;
    if (multiplier == 0)
        multiplier = 1;
    const int_fast8_t *table = Etc2AlphaModTable[data[1] & 0xf];
    uint_fast64_t l = bton64(*(uint64_t*)data);
    for (int i = 0; i < 16; i++, l >>= 3) {
        int_fast16_t val = base * 8 + multiplier * table[l & 7] + 1023;
        ((uint8_t *)(outbuf + WriteOrderTableRev[i]))[color] = val < 0 ? 0 : val >= 2048 ? 0xff : val >> 3;
    }
}

int decode_etc1(const uint8_t *data, const long w, const long h, uint32_t *image) {
    long num_blocks_x = (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    uint32_t buffer[16];
    for (long by = 0; by < num_blocks_y; by++) {
        for (long bx = 0; bx < num_blocks_x; bx++, data += 8) {
            decode_etc1_block(data, buffer);
            copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
        }
    }
    return 1;
}

int decode_etc2(const uint8_t *data, const long w, const long h, uint32_t *image) {
    long num_blocks_x = (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    uint32_t buffer[16];
    for (long by = 0; by < num_blocks_y; by++) {
        for (long bx = 0; bx < num_blocks_x; bx++, data += 8) {
            decode_etc2_block(data, buffer);
            copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
        }
    }
    return 1;
}

int decode_etc2a1(const uint8_t *data, const long w, const long h, uint32_t *image) {
    long num_blocks_x = (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    uint32_t buffer[16];
    for (long by = 0; by < num_blocks_y; by++) {
        for (long bx = 0; bx < num_blocks_x; bx++, data += 8) {
            decode_etc2a1_block(data, buffer);
            copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
        }
    }
    return 1;
}

int decode_etc2a8(const uint8_t *data, const long w, const long h, uint32_t *image) {
    long num_blocks_x = (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    uint32_t buffer[16];
    for (long by = 0; by < num_blocks_y; by++) {
        for (long bx = 0; bx < num_blocks_x; bx++, data += 16) {
            decode_etc2_block(data + 8, buffer);
            decode_etc2a8_block(data, buffer);
            copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
        }
    }
    return 1;
}

int decode_eacr(const uint8_t *data, const long w, const long h, uint32_t *image) {
    long num_blocks_x = (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    uint32_t buffer[16];
    uint32_t base_buffer[16];
    for (int i = 0; i < 16; i++)
        base_buffer[i] = color(0, 0, 0, 255);
    for (long by = 0; by < num_blocks_y; by++) {
        for (long bx = 0; bx < num_blocks_x; bx++, data += 8) {
            memcpy(buffer, base_buffer, sizeof(buffer));
            decode_eac_block(data, 2, buffer);
            copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
        }
    }
    return 1;
}

int decode_eacr_signed(const uint8_t *data, const long w, const long h, uint32_t *image) {
    long num_blocks_x = (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    uint32_t buffer[16];
    uint32_t base_buffer[16];
    for (int i = 0; i < 16; i++)
        base_buffer[i] = color(0, 0, 0, 255);
    for (long by = 0; by < num_blocks_y; by++) {
        for (long bx = 0; bx < num_blocks_x; bx++, data += 8) {
            memcpy(buffer, base_buffer, sizeof(buffer));
            decode_eac_signed_block(data, 2, buffer);
            copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
        }
    }
    return 1;
}

int decode_eacrg(const uint8_t *data, const long w, const long h, uint32_t *image) {
    long num_blocks_x = (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    uint32_t buffer[16];
    uint32_t base_buffer[16];
    for (int i = 0; i < 16; i++)
        base_buffer[i] = color(0, 0, 0, 255);
    for (long by = 0; by < num_blocks_y; by++) {
        for (long bx = 0; bx < num_blocks_x; bx++, data += 16) {
            memcpy(buffer, base_buffer, sizeof(buffer));
            decode_eac_block(data, 2, buffer);
            decode_eac_block(data + 8, 1, buffer);
            copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
        }
    }
    return 1;
}

int decode_eacrg_signed(const uint8_t *data, const long w, const long h, uint32_t *image) {
    long num_blocks_x = (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    uint32_t buffer[16];
    uint32_t base_buffer[16];
    for (int i = 0; i < 16; i++)
        base_buffer[i] = color(0, 0, 0, 255);
    for (long by = 0; by < num_blocks_y; by++) {
        for (long bx = 0; bx < num_blocks_x; bx++, data += 16) {
            memcpy(buffer, base_buffer, sizeof(buffer));
            decode_eac_signed_block(data, 2, buffer);
            decode_eac_signed_block(data + 8, 1, buffer);
            copy_block_buffer(bx, by, w, h, 4, 4, buffer, image);
        }
    }
    return 1;
}
