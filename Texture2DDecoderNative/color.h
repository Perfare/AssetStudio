#ifndef COLOR_H
#define COLOR_H

#include <stdint.h>
#include <string.h>
#include "endianness.h"

#ifdef __LITTLE_ENDIAN__
static const uint_fast32_t TRANSPARENT_MASK = 0x00ffffff;
#else
static const uint_fast32_t TRANSPARENT_MASK = 0xffffff00;
#endif

static inline uint_fast32_t color(uint8_t r, uint8_t g, uint8_t b, uint8_t a) {
#ifdef __LITTLE_ENDIAN__
    return b | g << 8 | r << 16 | a << 24;
#else
    return a | r << 8 | g << 16 | b << 24;
#endif
}

static inline uint_fast32_t alpha_mask(uint8_t a) {
#ifdef __LITTLE_ENDIAN__
    return TRANSPARENT_MASK | a << 24;
#else
    return TRANSPARENT_MASK | a;
#endif
}

static inline void rgb565_le(const uint16_t d, uint8_t *r, uint8_t *g, uint8_t *b) {
#ifdef __LITTLE_ENDIAN__
    *r = (d >> 8 & 0xf8) | (d >> 13);
    *g = (d >> 3 & 0xfc) | (d >> 9 & 3);
    *b = (d << 3) | (d >> 2 & 7);
#else
    *r = (d & 0xf8) | (d >> 5 & 7);
    *g = (d << 5 & 0xe0) | (d >> 11 & 0x1c) | (d >> 1 & 3);
    *b = (d >> 5 & 0xf8) | (d >> 10 & 0x7);
#endif
}

static inline void rgb565_be(const uint16_t d, uint8_t *r, uint8_t *g, uint8_t *b) {
#ifdef __BIG_ENDIAN__
    *r = (d >> 8 & 0xf8) | (d >> 13);
    *g = (d >> 3 & 0xfc) | (d >> 9 & 3);
    *b = (d << 3) | (d >> 2 & 7);
#else
    *r = (d & 0xf8) | (d >> 5 & 7);
    *g = (d << 5 & 0xe0) | (d >> 11 & 0x1c) | (d >> 1 & 3);
    *b = (d >> 5 & 0xf8) | (d >> 10 & 0x7);
#endif
}

static inline void rgb565_lep(const uint16_t d, uint8_t *c) {
#ifdef __LITTLE_ENDIAN__
    *(c++) = (d >> 8 & 0xf8) | (d >> 13);
    *(c++) = (d >> 3 & 0xfc) | (d >> 9 & 3);
    *(c++) = (d << 3) | (d >> 2 & 7);
#else
    *(c++) = (d & 0xf8) | (d >> 5 & 7);
    *(c++) = (d << 5 & 0xe0) | (d >> 11 & 0x1c) | (d >> 1 & 3);
    *(c++) = (d >> 5 & 0xf8) | (d >> 10 & 0x7);
#endif
}

static inline void rgb565_bep(const uint16_t d, uint8_t *c) {
#ifdef __BIG_ENDIAN__
    *(c++) = (d >> 8 & 0xf8) | (d >> 13);
    *(c++) = (d >> 3 & 0xfc) | (d >> 9 & 3);
    *(c++) = (d << 3) | (d >> 2 & 7);
#else
    *(c++) = (d & 0xf8) | (d >> 5 & 7);
    *(c++) = (d << 5 & 0xe0) | (d >> 11 & 0x1c) | (d >> 1 & 3);
    *(c++) = (d >> 5 & 0xf8) | (d >> 10 & 0x7);
#endif
}

static inline void copy_block_buffer(const long bx, const long by, const long w, const long h, const long bw,
                                     const long bh, const uint32_t *buffer, uint32_t *image) {
    long x = bw * bx;
    long xl = (bw * (bx + 1) > w ? w - bw * bx : bw) * 4;
    const uint32_t *buffer_end = buffer + bw * bh;
    for (long y = by * bh; buffer < buffer_end && y < h; buffer += bw, y++)
        memcpy(image + y * w + x, buffer, xl);
}

#endif /* end of include guard: COLOR_H */
