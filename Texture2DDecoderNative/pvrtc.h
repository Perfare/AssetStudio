#ifndef PVRTC_H
#define PVRTC_H

#include <stdint.h>

typedef struct {
    uint8_t r;
    uint8_t g;
    uint8_t b;
    uint8_t a;
} PVRTCTexelColor;

typedef struct {
    int r;
    int g;
    int b;
    int a;
} PVRTCTexelColorInt;

typedef struct {
    PVRTCTexelColor a;
    PVRTCTexelColor b;
    int8_t weight[32];
    uint32_t punch_through_flag;
} PVRTCTexelInfo;

int decode_pvrtc(const uint8_t *, const long, const long, uint32_t *, const int);

#endif /* end of include guard: PVRTC_H */
