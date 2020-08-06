#include "pvrtc.h"
#include <stdint.h>
#include <string.h>
#include "color.h"
#include "endianness.h"

static const int PVRTC1_STANDARD_WEIGHT[] = {0, 3, 5, 8};
static const int PVRTC1_PUNCHTHROUGH_WEIGHT[] = {0, 4, 4, 8};

static inline long morton_index(const long x, const long y, const long min_dim) {
    long offset = 0, shift = 0;
    for (long mask = 1; mask < min_dim; mask <<= 1, shift++)
        offset |= (((y & mask) | ((x & mask) << 1))) << shift;
    offset |= ((x | y) >> shift) << (shift * 2);
    return offset;
}

static void get_texel_colors(const uint8_t *data, PVRTCTexelInfo *info) {
    uint16_t ca = lton16(*(uint16_t *)(data + 4));
    uint16_t cb = lton16(*(uint16_t *)(data + 6));
    if (ca & 0x8000) {
        info->a.r = ca >> 10 & 0x1f;
        info->a.g = ca >> 5 & 0x1f;
        info->a.b = (ca & 0x1e) | (ca >> 4 & 1);
        info->a.a = 0xf;
    } else {
        info->a.r = (ca >> 7 & 0x1e) | (ca >> 11 & 1);
        info->a.g = (ca >> 3 & 0x1e) | (ca >> 7 & 1);
        info->a.b = (ca << 1 & 0x1c) | (ca >> 2 & 3);
        info->a.a = ca >> 11 & 0xe;
    }
    if (cb & 0x8000) {
        info->b.r = cb >> 10 & 0x1f;
        info->b.g = cb >> 5 & 0x1f;
        info->b.b = cb & 0x1f;
        info->b.a = 0xf;
    } else {
        info->b.r = (cb >> 7 & 0x1e) | (cb >> 11 & 1);
        info->b.g = (cb >> 3 & 0x1e) | (cb >> 7 & 1);
        info->b.b = (cb << 1 & 0x1e) | (cb >> 3 & 1);
        info->b.a = cb >> 11 & 0xe;
    }
}

static void get_texel_weights_4bpp(const uint8_t *data, PVRTCTexelInfo *info) {
    info->punch_through_flag = 0;

    int mod_mode = data[4] & 1;
    uint32_t mod_bits = lton32(*(uint32_t *)data);

    if (mod_mode) {
        for (int i = 0; i < 16; i++, mod_bits >>= 2) {
            info->weight[i] = PVRTC1_PUNCHTHROUGH_WEIGHT[mod_bits & 3];
            if ((mod_bits & 3) == 2)
                info->punch_through_flag |= 1 << i;
        }
    } else {
        for (int i = 0; i < 16; i++, mod_bits >>= 2)
            info->weight[i] = PVRTC1_STANDARD_WEIGHT[mod_bits & 3];
    }
}

static void get_texel_weights_2bpp(const uint8_t *data, PVRTCTexelInfo *info) {
    info->punch_through_flag = 0;

    int mod_mode = data[4] & 1;
    uint32_t mod_bits = lton32(*(uint32_t *)data);

    if (mod_mode) {
        int fillflag = data[0] & 1 ? (data[2] & 0x10 ? -1 : -2) : -3;
        for (int y = 0, i = 1; y < 4; ++y & 1 ? --i : ++i)
            for (int x = 0; x < 4; x++, i += 2)
                info->weight[i] = fillflag;
        for (int y = 0, i = 0; y < 4; ++y & 1 ? ++i : --i)
            for (int x = 0; x < 4; x++, i += 2, mod_bits >>= 2)
                info->weight[i] = PVRTC1_STANDARD_WEIGHT[mod_bits & 3];
        info->weight[0] = (info->weight[0] + 3) & 8;
        if (data[0] & 1)
            info->weight[20] = (info->weight[20] + 3) & 8;
    } else {
        for (int i = 0; i < 32; i++, mod_bits >>= 1)
            info->weight[i] = mod_bits & 1 ? 8 : 0;
    }
}

static void applicate_color_4bpp(const uint8_t *data, PVRTCTexelInfo *const info[9], uint32_t buf[32]) {
    static const int INTERP_WEIGHT[4][3] = {{2, 2, 0}, {1, 3, 0}, {0, 4, 0}, {0, 3, 1}};
    PVRTCTexelColorInt clr_a[16] = {}, clr_b[16] = {};

    for (int y = 0, i = 0; y < 4; y++) {
        for (int x = 0; x < 4; x++, i++) {
            for (int acy = 0, ac = 0; acy < 3; acy++) {
                for (int acx = 0; acx < 3; acx++, ac++) {
                    int interp_weight = INTERP_WEIGHT[x][acx] * INTERP_WEIGHT[y][acy];
                    clr_a[i].r += info[ac]->a.r * interp_weight;
                    clr_a[i].g += info[ac]->a.g * interp_weight;
                    clr_a[i].b += info[ac]->a.b * interp_weight;
                    clr_a[i].a += info[ac]->a.a * interp_weight;
                    clr_b[i].r += info[ac]->b.r * interp_weight;
                    clr_b[i].g += info[ac]->b.g * interp_weight;
                    clr_b[i].b += info[ac]->b.b * interp_weight;
                    clr_b[i].a += info[ac]->b.a * interp_weight;
                }
            }
            clr_a[i].r = (clr_a[i].r >> 1) + (clr_a[i].r >> 6);
            clr_a[i].g = (clr_a[i].g >> 1) + (clr_a[i].g >> 6);
            clr_a[i].b = (clr_a[i].b >> 1) + (clr_a[i].b >> 6);
            clr_a[i].a = (clr_a[i].a) + (clr_a[i].a >> 4);
            clr_b[i].r = (clr_b[i].r >> 1) + (clr_b[i].r >> 6);
            clr_b[i].g = (clr_b[i].g >> 1) + (clr_b[i].g >> 6);
            clr_b[i].b = (clr_b[i].b >> 1) + (clr_b[i].b >> 6);
            clr_b[i].a = (clr_b[i].a) + (clr_b[i].a >> 4);
        }
    }

    const PVRTCTexelInfo *self_info = info[4];
    uint32_t punch_through_flag = self_info->punch_through_flag;
    for (int i = 0; i < 16; i++, punch_through_flag >>= 1) {
        buf[i] = color((clr_a[i].r * (8 - self_info->weight[i]) + clr_b[i].r * self_info->weight[i]) / 8,
                       (clr_a[i].g * (8 - self_info->weight[i]) + clr_b[i].g * self_info->weight[i]) / 8,
                       (clr_a[i].b * (8 - self_info->weight[i]) + clr_b[i].b * self_info->weight[i]) / 8,
                       punch_through_flag & 1
                         ? 0
                         : (clr_a[i].a * (8 - self_info->weight[i]) + clr_b[i].a * self_info->weight[i]) / 8);
    }
}

static void applicate_color_2bpp(const uint8_t *data, PVRTCTexelInfo *const info[9], uint32_t buf[32]) {
    static const int INTERP_WEIGHT_X[8][3] = {{4, 4, 0}, {3, 5, 0}, {2, 6, 0}, {1, 7, 0},
                                              {0, 8, 0}, {0, 7, 1}, {0, 6, 2}, {0, 5, 3}};
    static const int INTERP_WEIGHT_Y[4][3] = {{2, 2, 0}, {1, 3, 0}, {0, 4, 0}, {0, 3, 1}};
    PVRTCTexelColorInt clr_a[32] = {}, clr_b[32] = {};

    for (int y = 0, i = 0; y < 4; y++) {
        for (int x = 0; x < 8; x++, i++) {
            for (int acy = 0, ac = 0; acy < 3; acy++) {
                for (int acx = 0; acx < 3; acx++, ac++) {
                    int interp_weight = INTERP_WEIGHT_X[x][acx] * INTERP_WEIGHT_Y[y][acy];
                    clr_a[i].r += info[ac]->a.r * interp_weight;
                    clr_a[i].g += info[ac]->a.g * interp_weight;
                    clr_a[i].b += info[ac]->a.b * interp_weight;
                    clr_a[i].a += info[ac]->a.a * interp_weight;
                    clr_b[i].r += info[ac]->b.r * interp_weight;
                    clr_b[i].g += info[ac]->b.g * interp_weight;
                    clr_b[i].b += info[ac]->b.b * interp_weight;
                    clr_b[i].a += info[ac]->b.a * interp_weight;
                }
            }
            clr_a[i].r = (clr_a[i].r >> 2) + (clr_a[i].r >> 7);
            clr_a[i].g = (clr_a[i].g >> 2) + (clr_a[i].g >> 7);
            clr_a[i].b = (clr_a[i].b >> 2) + (clr_a[i].b >> 7);
            clr_a[i].a = (clr_a[i].a >> 1) + (clr_a[i].a >> 5);
            clr_b[i].r = (clr_b[i].r >> 2) + (clr_b[i].r >> 7);
            clr_b[i].g = (clr_b[i].g >> 2) + (clr_b[i].g >> 7);
            clr_b[i].b = (clr_b[i].b >> 2) + (clr_b[i].b >> 7);
            clr_b[i].a = (clr_b[i].a >> 1) + (clr_b[i].a >> 5);
        }
    }

    static const int POSYA[4][2] = {{1, 24}, {4, -8}, {4, -8}, {4, -8}};
    static const int POSYB[4][2] = {{4, 8}, {4, 8}, {4, 8}, {7, -24}};
    static const int POSXL[8][2] = {{3, 7}, {4, -1}, {4, -1}, {4, -1}, {4, -1}, {4, -1}, {4, -1}, {4, -1}};
    static const int POSXR[8][2] = {{4, 1}, {4, 1}, {4, 1}, {4, 1}, {4, 1}, {4, 1}, {4, 1}, {5, -7}};

    PVRTCTexelInfo *self_info = info[4];
    uint32_t punch_through_flag = self_info->punch_through_flag;
    for (int y = 0, i = 0; y < 4; y++) {
        for (int x = 0; x < 8; x++, i++, punch_through_flag >>= 1) {
            switch (self_info->weight[i]) {
            case -1:
                self_info->weight[i] =
                  (info[POSYA[y][0]]->weight[i + POSYA[y][1]] + info[POSYB[y][0]]->weight[i + POSYB[y][1]] + 1) / 2;
                break;
            case -2:
                self_info->weight[i] =
                  (info[POSXL[x][0]]->weight[i + POSXL[x][1]] + info[POSXR[x][0]]->weight[i + POSXR[x][1]] + 1) / 2;
                break;
            case -3:
                self_info->weight[i] =
                  (info[POSYA[y][0]]->weight[i + POSYA[y][1]] + info[POSYB[y][0]]->weight[i + POSYB[y][1]] +
                   info[POSXL[x][0]]->weight[i + POSXL[x][1]] + info[POSXR[x][0]]->weight[i + POSXR[x][1]] + 2) /
                  4;
                break;
            }
            buf[i] = color((clr_a[i].r * (8 - self_info->weight[i]) + clr_b[i].r * self_info->weight[i]) / 8,
                           (clr_a[i].g * (8 - self_info->weight[i]) + clr_b[i].g * self_info->weight[i]) / 8,
                           (clr_a[i].b * (8 - self_info->weight[i]) + clr_b[i].b * self_info->weight[i]) / 8,
                           punch_through_flag & 1
                             ? 0
                             : (clr_a[i].a * (8 - self_info->weight[i]) + clr_b[i].a * self_info->weight[i]) / 8);
        }
    }
}

int decode_pvrtc(const uint8_t *data, const long w, const long h, uint32_t *image, const int is2bpp) {
    long bw = is2bpp ? 8 : 4;
    long num_blocks_x = is2bpp ? (w + 7) / 8 : (w + 3) / 4;
    long num_blocks_y = (h + 3) / 4;
    long num_blocks = num_blocks_x * num_blocks_y;
    long min_num_blocks = num_blocks_x <= num_blocks_y ? num_blocks_x : num_blocks_y;

    if ((num_blocks_x & (num_blocks_x - 1)) || (num_blocks_y & (num_blocks_y - 1))) {
        //extern const char* error_msg;
        //error_msg = "the number of blocks of each side must be a power of 2";
        return 0;
    }

    PVRTCTexelInfo *texel_info = (PVRTCTexelInfo *)malloc(sizeof(PVRTCTexelInfo) * num_blocks);
    if (texel_info == NULL) {
        //extern const char* error_msg;
        //error_msg = "memory allocation failed";
        return 0;
    }

    void (*get_texel_weights_func)(const uint8_t *, PVRTCTexelInfo *) =
      is2bpp ? get_texel_weights_2bpp : get_texel_weights_4bpp;
    void (*applicate_color_func)(const uint8_t *, PVRTCTexelInfo *const[9], uint32_t[32]) =
      is2bpp ? applicate_color_2bpp : applicate_color_4bpp;

    const uint8_t *d = data;
    for (long i = 0; i < num_blocks; i++, d += 8) {
        get_texel_colors(d, &texel_info[i]);
        get_texel_weights_func(d, &texel_info[i]);
    }

    uint32_t buffer[32];
    PVRTCTexelInfo *local_info[9];
    long pos_x[3], pos_y[3];

    for (long by = 0; by < num_blocks_y; by++) {
        pos_y[0] = by == 0 ? num_blocks_y - 1 : by - 1;
        pos_y[1] = by;
        pos_y[2] = by == num_blocks_y - 1 ? 0 : by + 1;
        for (long bx = 0, x = 0; bx < num_blocks_x; bx++, x += 4) {
            pos_x[0] = bx == 0 ? num_blocks_x - 1 : bx - 1;
            pos_x[1] = bx;
            pos_x[2] = bx == num_blocks_x - 1 ? 0 : bx + 1;
            for (long cy = 0, c = 0; cy < 3; cy++)
                for (long cx = 0; cx < 3; cx++, c++)
                    local_info[c] = &texel_info[morton_index(pos_x[cx], pos_y[cy], min_num_blocks)];
            applicate_color_func(data + morton_index(bx, by, min_num_blocks) * 8, local_info, buffer);
            copy_block_buffer(bx, by, w, h, bw, 4, buffer, image);
        }
    }

    free(texel_info);
    return 1;
}
