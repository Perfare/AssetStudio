#ifndef CRND_INCLUDE_CRN_DEFS_H
#define CRND_INCLUDE_CRN_DEFS_H

// Include crnlib.h (only to bring in some basic CRN-related types).
#include "crnlib.h"

#define CRND_LIB_VERSION 104
#define CRND_VERSION_STRING "01.04"

#ifdef _DEBUG
#define CRND_BUILD_DEBUG
#else
#define CRND_BUILD_RELEASE
#endif

// CRN decompression API
namespace unitycrnd {
typedef unsigned char uint8;
typedef signed char int8;
typedef unsigned short uint16;
typedef signed short int16;
typedef unsigned int uint32;
typedef uint32 uint32;
typedef unsigned int uint;
typedef signed int int32;
#ifdef __GNUC__
typedef unsigned long long uint64;
typedef long long int64;
#else
typedef unsigned __int64 uint64;
typedef signed __int64 int64;
#endif

// The crnd library assumes all allocation blocks have at least CRND_MIN_ALLOC_ALIGNMENT alignment.
const uint32 CRND_MIN_ALLOC_ALIGNMENT = sizeof(uint32) * 2U;

// realloc callback:
// Used to allocate, resize, or free memory blocks.
// If p is NULL, the realloc function attempts to allocate a block of at least size bytes. Returns NULL on out of memory.
// *pActual_size must be set to the actual size of the allocated block, which must be greater than or equal to the requested size.
// If p is not NULL, and size is 0, the realloc function frees the specified block, and always returns NULL. *pActual_size should be set to 0.
// If p is not NULL, and size is non-zero, the realloc function attempts to resize the specified block:
//    If movable is false, the realloc function attempts to shrink or expand the block in-place. NULL is returned if the block cannot be resized in place, or if the
//    underlying heap implementation doesn't support in-place resizing. Otherwise, the pointer to the original block is returned.
//    If movable is true, it is permissible to move the block's contents if it cannot be resized in place. NULL is returned if the block cannot be resized in place, and there
//    is not enough memory to relocate the block.
//    In all cases, *pActual_size must be set to the actual size of the allocated block, whether it was successfully resized or not.
typedef void* (*crnd_realloc_func)(void* p, size_t size, size_t* pActual_size, bool movable, void* pUser_data);

// msize callback: Returns the size of the memory block in bytes, or 0 if the pointer or block is invalid.
typedef size_t (*crnd_msize_func)(void* p, void* pUser_data);

// crnd_set_memory_callbacks() - Use to override the crnd library's memory allocation functions.
// If any input parameters are NULL, the memory callback functions are reset to the default functions.
// The default functions call malloc(), free(),  _msize(), _expand(), etc.
void crnd_set_memory_callbacks(crnd_realloc_func pRealloc, crnd_msize_func pMSize, void* pUser_data);

struct crn_file_info {
  inline crn_file_info()
      : m_struct_size(sizeof(crn_file_info)) {}

  uint32 m_struct_size;
  uint32 m_actual_data_size;
  uint32 m_header_size;
  uint32 m_total_palette_size;
  uint32 m_tables_size;
  uint32 m_levels;
  uint32 m_level_compressed_size[cCRNMaxLevels];
  uint32 m_color_endpoint_palette_entries;
  uint32 m_color_selector_palette_entries;
  uint32 m_alpha_endpoint_palette_entries;
  uint32 m_alpha_selector_palette_entries;
};

struct crn_texture_info {
  inline crn_texture_info()
      : m_struct_size(sizeof(crn_texture_info)) {}

  uint32 m_struct_size;
  uint32 m_width;
  uint32 m_height;
  uint32 m_levels;
  uint32 m_faces;
  uint32 m_bytes_per_block;
  uint32 m_userdata0;
  uint32 m_userdata1;
  crn_format m_format;
};

struct crn_level_info {
  inline crn_level_info()
      : m_struct_size(sizeof(crn_level_info)) {}

  uint32 m_struct_size;
  uint32 m_width;
  uint32 m_height;
  uint32 m_faces;
  uint32 m_blocks_x;
  uint32 m_blocks_y;
  uint32 m_bytes_per_block;
  crn_format m_format;
};

// Returns the FOURCC format code corresponding to the specified CRN format.
uint32 crnd_crn_format_to_fourcc(crn_format fmt);

// Returns the fundamental GPU format given a potentially swizzled DXT5 crn_format.
crn_format crnd_get_fundamental_dxt_format(crn_format fmt);

// Returns the size of the crn_format in bits/texel (either 4 or 8).
uint32 crnd_get_crn_format_bits_per_texel(crn_format fmt);

// Returns the number of bytes per DXTn block (8 or 16).
uint32 crnd_get_bytes_per_dxt_block(crn_format fmt);

// Validates the entire file by checking the header and data CRC's.
// This is not something you want to be doing much!
// The crn_file_info.m_struct_size field must be set before calling this function.
bool crnd_validate_file(const void* pData, uint32 data_size, crn_file_info* pFile_info);

// Retrieves texture information from the CRN file.
// The crn_texture_info.m_struct_size field must be set before calling this function.
bool crnd_get_texture_info(const void* pData, uint32 data_size, crn_texture_info* pTexture_info);

// Retrieves mipmap level specific information from the CRN file.
// The crn_level_info.m_struct_size field must be set before calling this function.
bool crnd_get_level_info(const void* pData, uint32 data_size, uint32 level_index, crn_level_info* pLevel_info);

// Transcode/unpack context handle.
typedef void* crnd_unpack_context;

// crnd_unpack_begin() - Decompresses the texture's decoder tables and endpoint/selector palettes.
// Once you call this function, you may call crnd_unpack_level() to unpack one or more mip levels.
// Don't call this once per mip level (unless you absolutely must)!
// This function allocates enough memory to hold: Huffman decompression tables, and the endpoint/selector palettes (color and/or alpha).
// Worst case allocation is approx. 200k, assuming all palettes contain 8192 entries.
// pData must point to a buffer holding all of the compressed .CRN file data.
// This buffer must be stable until crnd_unpack_end() is called.
// Returns NULL if out of memory, or if any of the input parameters are invalid.
crnd_unpack_context crnd_unpack_begin(const void* pData, uint32 data_size);

// Returns a pointer to the compressed .CRN data associated with a crnd_unpack_context.
// Returns false if any of the input parameters are invalid.
bool crnd_get_data(crnd_unpack_context pContext, const void** ppData, uint32* pData_size);

// crnd_unpack_level() - Transcodes the specified mipmap level to a destination buffer in cached or write combined memory.
// pContext - Context created by a call to crnd_unpack_begin().
// ppDst - A pointer to an array of 1 or 6 destination buffer pointers. Cubemaps require an array of 6 pointers, 2D textures require an array of 1 pointer.
// dst_size_in_bytes - Optional size of each destination buffer. Only used for debugging - OK to set to UINT32_MAX.
// row_pitch_in_bytes - The pitch in bytes from one row of DXT blocks to the next. Must be a multiple of 4.
// level_index - mipmap level index, where 0 is the largest/first level.
// Returns false if any of the input parameters, or the compressed stream, are invalid.
// This function does not allocate any memory.
bool crnd_unpack_level(
    crnd_unpack_context pContext,
    void** ppDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes,
    uint32 level_index);

// crnd_unpack_level_segmented() - Unpacks the specified mipmap level from a "segmented" CRN file.
// See the crnd_create_segmented_file() API below.
// Segmented files allow the user to control where the compressed mipmap data is stored.
bool crnd_unpack_level_segmented(
    crnd_unpack_context pContext,
    const void* pSrc, uint32 src_size_in_bytes,
    void** ppDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes,
    uint32 level_index);

// crnd_unpack_end() - Frees the decompress tables and unpacked palettes associated with the specified unpack context.
// Returns false if the context is NULL, or if it points to an invalid context.
// This function frees all memory associated with the context.
bool crnd_unpack_end(crnd_unpack_context pContext);

// The following API's allow the user to create "segmented" CRN files. A segmented file contains multiple pieces:
// - Base data: Header + compression tables
// - Level data: Individual mipmap levels
// This allows mipmap levels from multiple CRN files to be tightly packed together into single files.

// Returns a pointer to the level's compressed data, and optionally returns the level's compressed data size if pSize is not NULL.
const void* crnd_get_level_data(const void* pData, uint32 data_size, uint32 level_index, uint32* pSize);

// Returns the compressed size of the texture's header and compression tables (but no levels).
uint32 crnd_get_segmented_file_size(const void* pData, uint32 data_size);

// Creates a "segmented" CRN texture from a normal CRN texture. The new texture will be created at pBase_data, and will be crnd_get_base_data_size() bytes long.
// base_data_size must be >= crnd_get_base_data_size().
// The base data will contain the CRN header and compression tables, but no mipmap data.
bool crnd_create_segmented_file(const void* pData, uint32 data_size, void* pBase_data, uint base_data_size);

}  // namespace unitycrnd

// Low-level CRN file header cracking.
namespace unitycrnd {
template <unsigned int N>
struct crn_packed_uint {
  inline crn_packed_uint() {}

  inline crn_packed_uint(unsigned int val) { *this = val; }

  inline crn_packed_uint(const crn_packed_uint& other) { *this = other; }

  inline crn_packed_uint& operator=(const crn_packed_uint& rhs) {
    if (this != &rhs)
      memcpy(m_buf, rhs.m_buf, sizeof(m_buf));
    return *this;
  }

  inline crn_packed_uint& operator=(unsigned int val) {
    //CRND_ASSERT((N == 4U) || (val < (1U << (N * 8U))));

    val <<= (8U * (4U - N));

    for (unsigned int i = 0; i < N; i++) {
      m_buf[i] = static_cast<unsigned char>(val >> 24U);
      val <<= 8U;
    }

    return *this;
  }

  inline operator unsigned int() const {
    switch (N) {
      case 1:
        return m_buf[0];
      case 2:
        return (m_buf[0] << 8U) | m_buf[1];
      case 3:
        return (m_buf[0] << 16U) | (m_buf[1] << 8U) | (m_buf[2]);
      default:
        return (m_buf[0] << 24U) | (m_buf[1] << 16U) | (m_buf[2] << 8U) | (m_buf[3]);
    }
  }

  unsigned char m_buf[N];
};

#pragma pack(push)
#pragma pack(1)
struct crn_palette {
  crn_packed_uint<3> m_ofs;
  crn_packed_uint<3> m_size;
  crn_packed_uint<2> m_num;
};

enum crn_header_flags {
  // If set, the compressed mipmap level data is not located after the file's base data - it will be separately managed by the user instead.
  cCRNHeaderFlagSegmented = 1
};

struct crn_header {
  enum { cCRNSigValue = ('H' << 8) | 'x' };

  crn_packed_uint<2> m_sig;
  crn_packed_uint<2> m_header_size;
  crn_packed_uint<2> m_header_crc16;

  crn_packed_uint<4> m_data_size;
  crn_packed_uint<2> m_data_crc16;

  crn_packed_uint<2> m_width;
  crn_packed_uint<2> m_height;

  crn_packed_uint<1> m_levels;
  crn_packed_uint<1> m_faces;

  crn_packed_uint<1> m_format;
  crn_packed_uint<2> m_flags;

  crn_packed_uint<4> m_reserved;
  crn_packed_uint<4> m_userdata0;
  crn_packed_uint<4> m_userdata1;

  crn_palette m_color_endpoints;
  crn_palette m_color_selectors;

  crn_palette m_alpha_endpoints;
  crn_palette m_alpha_selectors;

  crn_packed_uint<2> m_tables_size;
  crn_packed_uint<3> m_tables_ofs;

  // m_level_ofs[] is actually an array of offsets: m_level_ofs[m_levels]
  crn_packed_uint<4> m_level_ofs[1];
};

const unsigned int cCRNHeaderMinSize = 62U;

#pragma pack(pop)

}  // namespace unitycrnd

#endif  // CRND_INCLUDE_CRN_DEFS_H
