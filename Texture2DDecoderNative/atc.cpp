#include "bcn.h"
#include "atc.h"
#include "color.h"
#include <algorithm>

static uint8_t expand_quantized(uint8_t v, int bits) {
	v = v << (8 - bits);
	return v | (v >> bits);
}

void decode_atc_block(const uint8_t* _src, uint32_t* _dst)
{
	uint8_t colors[4 * 4];

	uint32_t c0 = _src[0] | (_src[1] << 8);
	uint32_t c1 = _src[2] | (_src[3] << 8);

	if (0 == (c0 & 0x8000))
	{
		colors[0] = expand_quantized((c0 >> 0) & 0x1f, 5);
		colors[1] = expand_quantized((c0 >> 5) & 0x1f, 5);
		colors[2] = expand_quantized((c0 >> 10) & 0x1f, 5);

		colors[12] = expand_quantized((c1 >> 0) & 0x1f, 5);
		colors[13] = expand_quantized((c1 >> 5) & 0x3f, 6);
		colors[14] = expand_quantized((c1 >> 11) & 0x1f, 5);

		colors[4] = (5 * colors[0] + 3 * colors[12]) / 8;
		colors[5] = (5 * colors[1] + 3 * colors[13]) / 8;
		colors[6] = (5 * colors[2] + 3 * colors[14]) / 8;

		colors[8] = (3 * colors[0] + 5 * colors[12]) / 8;
		colors[9] = (3 * colors[1] + 5 * colors[13]) / 8;
		colors[10] = (3 * colors[2] + 5 * colors[14]) / 8;
	}
	else
	{
		colors[0] = 0;
		colors[1] = 0;
		colors[2] = 0;

		colors[8] = expand_quantized((c0 >> 0) & 0x1f, 5);
		colors[9] = expand_quantized((c0 >> 5) & 0x1f, 5);
		colors[10] = expand_quantized((c0 >> 10) & 0x1f, 5);

		colors[12] = expand_quantized((c1 >> 0) & 0x1f, 5);
		colors[13] = expand_quantized((c1 >> 5) & 0x3f, 6);
		colors[14] = expand_quantized((c1 >> 11) & 0x1f, 5);

		colors[4] = std::max(0, colors[8] - colors[12] / 4);
		colors[5] = std::max(0, colors[9] - colors[13] / 4);
		colors[6] = std::max(0, colors[10] - colors[14] / 4);
	}

	for (uint32_t i = 0, next = 8 * 4; i < 16; i += 1, next += 2)
	{
		int32_t idx = ((_src[next >> 3] >> (next & 7)) & 3) * 4;
		_dst[i] = color(colors[idx + 2], colors[idx + 1], colors[idx + 0], 255);
	}
}

int decode_atc_rgb4(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image) {
	uint32_t m_block_width = 4;
	uint32_t m_block_height = 4;
	uint32_t m_blocks_x = (m_width + m_block_width - 1) / m_block_width;
	uint32_t m_blocks_y = (m_height + m_block_height - 1) / m_block_height;
	uint32_t buffer[16];
	for (uint32_t by = 0; by < m_blocks_y; by++) {
		for (uint32_t bx = 0; bx < m_blocks_x; bx++, data += 8) {
			decode_atc_block(data, buffer);
			copy_block_buffer(bx, by, m_width, m_height, m_block_width, m_block_height, buffer, image);
		}
	}
	return 1;
}

int decode_atc_rgba8(const uint8_t* data, uint32_t m_width, uint32_t m_height, uint32_t* image) {
	uint32_t m_block_width = 4;
	uint32_t m_block_height = 4;
	uint32_t m_blocks_x = (m_width + m_block_width - 1) / m_block_width;
	uint32_t m_blocks_y = (m_height + m_block_height - 1) / m_block_height;
	uint32_t buffer[16];
	for (uint32_t by = 0; by < m_blocks_y; by++) {
		for (uint32_t bx = 0; bx < m_blocks_x; bx++, data += 16) {
			decode_atc_block(data + 8, buffer);
			decode_bc3_alpha(data, buffer, 3);
			copy_block_buffer(bx, by, m_width, m_height, m_block_width, m_block_height, buffer, image);
		}
	}
	return 1;
}