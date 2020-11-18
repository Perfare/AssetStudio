#ifndef ASTC_H
#define ASTC_H

#include <stdint.h>

int decode_astc(const uint8_t *, const long, const long, const int, const int, uint32_t *);

#endif /* end of include guard: ASTC_H */
