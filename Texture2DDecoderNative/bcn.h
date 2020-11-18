#pragma once
#include <stdint.h>

struct color_bgra
{
	uint8_t b;
	uint8_t g;
	uint8_t r;
	uint8_t a;
};

const color_bgra g_black_color{ 0, 0, 0, 255 };

int decode_bc1(const uint8_t* data, const long w, const long h, uint32_t* image);
void decode_bc3_alpha(const uint8_t* data, uint32_t* outbuf, int channel);
int decode_bc3(const uint8_t* data, const long w, const long h, uint32_t* image);
int decode_bc4(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image);
int decode_bc5(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image);
int decode_bc6(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image);
int decode_bc7(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image);