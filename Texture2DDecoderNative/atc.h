#pragma once
#include <stdint.h>

int decode_atc_rgb4(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image);
int decode_atc_rgba8(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image);