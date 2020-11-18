// File: crn_decomp.h - Fast CRN->DXTc texture transcoder header file library
// Copyright (c) 2010-2016 Richard Geldreich, Jr. All rights reserved.
// See Copyright Notice and license at the end of this file.
//
// This single header file contains *all* of the code necessary to unpack .CRN files to raw DXTn bits.
// It does NOT depend on the crn compression library.
//
// Note: This is a single file, stand-alone C++ library which is controlled by the use of two macros:
//   If CRND_INCLUDE_CRND_H is NOT defined, the header is included.
//   If CRND_HEADER_FILE_ONLY is NOT defined, the implementation is included.
//
// Important: If compiling with gcc, be sure strict aliasing is disabled: -fno-strict-aliasing
#ifndef CRND_INCLUDE_CRND_H
#define CRND_INCLUDE_CRND_H

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
namespace crnd
{
   typedef unsigned char      uint8;
   typedef signed char        int8;
   typedef unsigned short     uint16;
   typedef signed short       int16;
   typedef unsigned int       uint32;
   typedef uint32             uint32;
   typedef unsigned int       uint;
   typedef signed int         int32;
   #ifdef __GNUC__
      typedef unsigned long long    uint64;
      typedef long long             int64;
   #else
      typedef unsigned __int64      uint64;
      typedef signed __int64        int64;
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
   typedef void*  (*crnd_realloc_func)(void* p, size_t size, size_t* pActual_size, bool movable, void* pUser_data);

   // msize callback: Returns the size of the memory block in bytes, or 0 if the pointer or block is invalid.
   typedef size_t (*crnd_msize_func)(void* p, void* pUser_data);

   // crnd_set_memory_callbacks() - Use to override the crnd library's memory allocation functions.
   // If any input parameters are NULL, the memory callback functions are reset to the default functions.
   // The default functions call malloc(), free(),  _msize(), _expand(), etc.
   void crnd_set_memory_callbacks(crnd_realloc_func pRealloc, crnd_msize_func pMSize, void* pUser_data);

   struct crn_file_info
   {
      inline crn_file_info() : m_struct_size(sizeof(crn_file_info)) { }

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

   struct crn_texture_info
   {
      inline crn_texture_info() : m_struct_size(sizeof(crn_texture_info)) { }

      uint32      m_struct_size;
      uint32      m_width;
      uint32      m_height;
      uint32      m_levels;
      uint32      m_faces;
      uint32      m_bytes_per_block;
      uint32      m_userdata0;
      uint32      m_userdata1;
      crn_format  m_format;
   };

   struct crn_level_info
   {
      inline crn_level_info() : m_struct_size(sizeof(crn_level_info)) { }

      uint32      m_struct_size;
      uint32      m_width;
      uint32      m_height;
      uint32      m_faces;
      uint32      m_blocks_x;
      uint32      m_blocks_y;
      uint32      m_bytes_per_block;
      crn_format  m_format;
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

} // namespace crnd

// Low-level CRN file header cracking.
namespace crnd
{
   template<unsigned int N>
   struct crn_packed_uint
   {
      inline crn_packed_uint() { }

      inline crn_packed_uint(unsigned int val) { *this = val; }

      inline crn_packed_uint(const crn_packed_uint& other) { *this = other; }

      inline crn_packed_uint& operator= (const crn_packed_uint& rhs)
      {
         if (this != &rhs)
            memcpy(m_buf, rhs.m_buf, sizeof(m_buf));
         return *this;
      }

      inline crn_packed_uint& operator= (unsigned int val)
      {
         //CRND_ASSERT((N == 4U) || (val < (1U << (N * 8U))));

         val <<= (8U * (4U - N));

         for (unsigned int i = 0; i < N; i++)
         {
            m_buf[i] = static_cast<unsigned char>(val >> 24U);
            val <<= 8U;
         }

         return *this;
      }

      inline operator unsigned int() const
      {
         switch (N)
         {
         case 1:  return  m_buf[0];
         case 2:  return (m_buf[0] <<  8U) |  m_buf[1];
         case 3:  return (m_buf[0] << 16U) | (m_buf[1] <<  8U) | (m_buf[2]);
         default: return (m_buf[0] << 24U) | (m_buf[1] << 16U) | (m_buf[2] << 8U) | (m_buf[3]);
         }
      }

      unsigned char m_buf[N];
   };

#pragma pack(push)
#pragma pack(1)
   struct crn_palette
   {
      crn_packed_uint<3> m_ofs;
      crn_packed_uint<3> m_size;
      crn_packed_uint<2> m_num;
   };

   enum crn_header_flags
   {
      // If set, the compressed mipmap level data is not located after the file's base data - it will be separately managed by the user instead.
      cCRNHeaderFlagSegmented = 1
   };

   struct crn_header
   {
      enum { cCRNSigValue = ('H' << 8) | 'x' };

      crn_packed_uint<2>    m_sig;
      crn_packed_uint<2>    m_header_size;
      crn_packed_uint<2>    m_header_crc16;

      crn_packed_uint<4>    m_data_size;
      crn_packed_uint<2>    m_data_crc16;

      crn_packed_uint<2>    m_width;
      crn_packed_uint<2>    m_height;

      crn_packed_uint<1>    m_levels;
      crn_packed_uint<1>    m_faces;

      crn_packed_uint<1>    m_format;
      crn_packed_uint<2>    m_flags;

      crn_packed_uint<4>    m_reserved;
      crn_packed_uint<4>    m_userdata0;
      crn_packed_uint<4>    m_userdata1;

      crn_palette           m_color_endpoints;
      crn_palette           m_color_selectors;

      crn_palette           m_alpha_endpoints;
      crn_palette           m_alpha_selectors;

      crn_packed_uint<2>    m_tables_size;
      crn_packed_uint<3>    m_tables_ofs;

      // m_level_ofs[] is actually an array of offsets: m_level_ofs[m_levels]
      crn_packed_uint<4>    m_level_ofs[1];
   };

   const unsigned int cCRNHeaderMinSize = 62U;

#pragma pack(pop)

} // namespace crnd

#endif // CRND_INCLUDE_CRND_H

// Internal library source follows this line.

#ifndef CRND_HEADER_FILE_ONLY

#include <stdlib.h>
#include <stdio.h>
#ifdef _WIN32
#include <memory.h>
#else
#include <malloc.h>
#endif
#include <stdarg.h>
#include <new> // needed for placement new, _msize, _expand

#define CRND_RESTRICT __restrict

#ifdef _MSC_VER
#include <intrin.h>
#pragma intrinsic(_WriteBarrier)
#pragma intrinsic(_ReadWriteBarrier)
#define CRND_WRITE_BARRIER _WriteBarrier();
#define CRND_FULL_BARRIER _ReadWriteBarrier();
#else
#define CRND_WRITE_BARRIER
#define CRND_FULL_BARRIER
#endif

#ifdef _MSC_VER
#pragma warning(disable:4127) // warning C4127: conditional expression is constant
#endif

#ifdef CRND_DEVEL
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x500
#endif
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#ifndef
#define NOMINMAX
#endif
#include "windows.h" // only for IsDebuggerPresent(), DebugBreak(), and OutputDebugStringA()
#endif

// File: crnd_types.h
namespace crnd
{
   const crn_uint8 cUINT8_MIN  = 0;
   const crn_uint8 cUINT8_MAX  = 0xFFU;
   const uint16 cUINT16_MIN = 0;
   const uint16 cUINT16_MAX = 0xFFFFU;
   const uint32 cUINT32_MIN = 0;
   const uint32 cUINT32_MAX = 0xFFFFFFFFU;

   const int8  cINT8_MIN  = -128;
   const int8  cINT8_MAX  = 127;
   const int16 cINT16_MIN = -32768;
   const int16 cINT16_MAX = 32767;
   const int32 cINT32_MIN = (-2147483647 - 1);
   const int32 cINT32_MAX = 2147483647;

   enum eClear { cClear };

   const uint32 cIntBits = 32U;

#ifdef _WIN64
   typedef uint64 ptr_bits;
#else
   #ifdef __x86_64__
      typedef uint64 ptr_bits;
   #else
      typedef uint32 ptr_bits;
   #endif
#endif

   template<typename T> struct int_traits { enum { cMin = crnd::cINT32_MIN, cMax = crnd::cINT32_MAX, cSigned = true }; };

   template<> struct int_traits<int8> { enum { cMin = crnd::cINT8_MIN, cMax = crnd::cINT8_MAX, cSigned = true }; };
   template<> struct int_traits<int16> { enum { cMin = crnd::cINT16_MIN, cMax = crnd::cINT16_MAX, cSigned = true }; };
   template<> struct int_traits<int32> { enum { cMin = crnd::cINT32_MIN, cMax = crnd::cINT32_MAX, cSigned = true }; };

   template<> struct int_traits<uint8> { enum { cMin = 0, cMax = crnd::cUINT8_MAX, cSigned = false }; };
   template<> struct int_traits<uint16> { enum { cMin = 0, cMax = crnd::cUINT16_MAX, cSigned = false }; };
   template<> struct int_traits<uint32> { enum { cMin = 0, cMax = crnd::cUINT32_MAX, cSigned = false }; };

   struct empty_type { };

} // namespace crnd

// File: crnd_platform.h
namespace crnd
{
#ifdef _XBOX
   const bool c_crnd_little_endian_platform = false;
   const bool c_crnd_big_endian_platform = true;
#define CRND_BIG_ENDIAN_PLATFORM 1
#else
   const bool c_crnd_little_endian_platform = true;
   const bool c_crnd_big_endian_platform = false;
#endif

   bool crnd_is_debugger_present();
   void crnd_debug_break();
   void crnd_output_debug_string(const char* p);

   // actually in crnd_assert.cpp
   void crnd_assert(const char* pExp, const char* pFile, unsigned line);
   void crnd_fail(const char* pExp, const char* pFile, unsigned line);

} // namespace crnd

// File: crnd_assert.h
namespace crnd
{
   void crnd_assert(const char* pExp, const char* pFile, unsigned line);

#ifdef NDEBUG
#define CRND_ASSERT(x) ((void)0)
#undef  CRND_ASSERTS_ENABLED
#else
#define CRND_ASSERT(_exp) (void)( (!!(_exp)) || (crnd::crnd_assert(#_exp, __FILE__, __LINE__), 0) )
#define CRND_ASSERTS_ENABLED
#endif

   void crnd_trace(const char* pFmt, va_list args);
   void crnd_trace(const char* pFmt, ...);

} // namespace crnd

// File: crnd_helpers.h
namespace crnd
{
   namespace helpers
   {
      template<typename T> struct rel_ops
      {
         friend bool operator!= (const T& x, const T& y) { return (!(x == y)); }
         friend bool operator>  (const T& x, const T& y) { return (y < x); }
         friend bool operator<= (const T& x, const T& y) { return (!(y < x)); }
         friend bool operator>= (const T& x, const T& y) { return (!(x < y)); }
      };

      template <typename T>
      inline T* construct(T* p)
      {
         return new (static_cast<void*>(p)) T;
      }

      template <typename T, typename U>
      inline T* construct(T* p, const U& init)
      {
         return new (static_cast<void*>(p)) T(init);
      }

      template <typename T>
      void construct_array(T* p, uint32 n)
      {
         T* q = p + n;
         for ( ; p != q; ++p)
            new (static_cast<void*>(p)) T;
      }

      template <typename T, typename U>
      void construct_array(T* p, uint32 n, const U& init)
      {
         T* q = p + n;
         for ( ; p != q; ++p)
            new (static_cast<void*>(p)) T(init);
      }

      template <typename T>
      inline void destruct(T* p)
      {
         p;
         p->~T();
      }

      template <typename T> inline void destruct_array(T* p, uint32 n)
      {
         T* q = p + n;
         for ( ; p != q; ++p)
            p->~T();
      }

   }  // namespace helpers

}  // namespace crnd

// File: crnd_traits.h
namespace crnd
{
   template<typename T>
   struct scalar_type
   {
      enum { cFlag = false };
      static inline void construct(T* p) { helpers::construct(p); }
      static inline void construct(T* p, const T& init) { helpers::construct(p, init); }
      static inline void construct_array(T* p, uint32 n) { helpers::construct_array(p, n); }
      static inline void destruct(T* p) { helpers::destruct(p); }
      static inline void destruct_array(T* p, uint32 n) { helpers::destruct_array(p, n); }
   };

   template<typename T> struct scalar_type<T*>
   {
      enum { cFlag = true };
      static inline void construct(T** p) { memset(p, 0, sizeof(T*)); }
      static inline void construct(T** p, T* init) { *p = init; }
      static inline void construct_array(T** p, uint32 n) { memset(p, 0, sizeof(T*) * n); }
      static inline void destruct(T** p) { p; }
      static inline void destruct_array(T** p, uint32 n) { p, n; }
   };

#define CRND_DEFINE_BUILT_IN_TYPE(X) \
   template<> struct scalar_type<X> { \
   enum { cFlag = true }; \
   static inline void construct(X* p) { memset(p, 0, sizeof(X)); } \
   static inline void construct(X* p, const X& init) { memcpy(p, &init, sizeof(X)); } \
   static inline void construct_array(X* p, uint32 n) { memset(p, 0, sizeof(X) * n); } \
   static inline void destruct(X* p) { p; } \
   static inline void destruct_array(X* p, uint32 n) { p, n; } };

   CRND_DEFINE_BUILT_IN_TYPE(bool)
   CRND_DEFINE_BUILT_IN_TYPE(char)
   CRND_DEFINE_BUILT_IN_TYPE(unsigned char)
   CRND_DEFINE_BUILT_IN_TYPE(short)
   CRND_DEFINE_BUILT_IN_TYPE(unsigned short)
   CRND_DEFINE_BUILT_IN_TYPE(int)
   CRND_DEFINE_BUILT_IN_TYPE(unsigned int)
   CRND_DEFINE_BUILT_IN_TYPE(long)
   CRND_DEFINE_BUILT_IN_TYPE(unsigned long)
   CRND_DEFINE_BUILT_IN_TYPE(int64)
   CRND_DEFINE_BUILT_IN_TYPE(uint64)
   CRND_DEFINE_BUILT_IN_TYPE(float)
   CRND_DEFINE_BUILT_IN_TYPE(double)
   CRND_DEFINE_BUILT_IN_TYPE(long double)

#undef CRND_DEFINE_BUILT_IN_TYPE

   // See: http://erdani.org/publications/cuj-2004-06.pdf

   template<typename T>
   struct bitwise_movable { enum { cFlag = false }; };

   // Defines type Q as bitwise movable.
#define CRND_DEFINE_BITWISE_MOVABLE(Q) template<> struct bitwise_movable<Q> { enum { cFlag = true }; };

   // From yasli_traits.h:
   // Credit goes to Boost;
   // also found in the C++ Templates book by Vandevoorde and Josuttis

   typedef char (&yes_t)[1];
   typedef char (&no_t)[2];

   template <class U> yes_t class_test(int U::*);
   template <class U> no_t class_test(...);

   template <class T> struct is_class
   {
      enum { value = (sizeof(class_test<T>(0)) == sizeof(yes_t)) };
   };

   template <typename T> struct is_pointer
   {
      enum { value = false };
   };

   template <typename T> struct is_pointer<T*>
   {
      enum { value = true };
   };

#define CRND_IS_POD(T) __is_pod(T)

} // namespace crnd

// File: crnd_mem.h
namespace crnd
{
   void*    crnd_malloc(size_t size, size_t* pActual_size = NULL);
   void*    crnd_realloc(void* p, size_t size, size_t* pActual_size = NULL, bool movable = true);
   void     crnd_free(void* p);
   size_t   crnd_msize(void* p);

   template<typename T>
   inline T* crnd_new()
   {
      T* p = static_cast<T*>(crnd_malloc(sizeof(T)));
      if (!p)
         return NULL;

      return helpers::construct(p);
   }

   template<typename T>
   inline T* crnd_new(const T& init)
   {
      T* p = static_cast<T*>(crnd_malloc(sizeof(T)));
      if (!p)
         return NULL;

      return helpers::construct(p, init);
   }

   template<typename T>
   inline T* crnd_new_array(uint32 num)
   {
      if (!num) num = 1;

      uint8* q = static_cast<uint8*>(crnd_malloc(CRND_MIN_ALLOC_ALIGNMENT + sizeof(T) * num));
      if (!q)
         return NULL;

      T* p = reinterpret_cast<T*>(q + CRND_MIN_ALLOC_ALIGNMENT);

      reinterpret_cast<uint32*>(p)[-1] = num;
      reinterpret_cast<uint32*>(p)[-2] = ~num;

      helpers::construct_array(p, num);
      return p;
   }

   template<typename T>
   inline void crnd_delete(T* p)
   {
      if (p)
      {
         helpers::destruct(p);
         crnd_free(p);
      }
   }

   template<typename T>
   inline void crnd_delete_array(T* p)
   {
      if (p)
      {
         const uint32 num = reinterpret_cast<uint32*>(p)[-1];
         const uint32 num_check = reinterpret_cast<uint32*>(p)[-2];
         num_check;
         CRND_ASSERT(num && (num == ~num_check));

         helpers::destruct_array(p, num);

         crnd_free(reinterpret_cast<uint8*>(p) - CRND_MIN_ALLOC_ALIGNMENT);
      }
   }

} // namespace crnd

// File: crnd_math.h
namespace crnd
{
   namespace math
   {
      const float cNearlyInfinite = 1.0e+37f;

      const float cDegToRad = 0.01745329252f;
      const float cRadToDeg = 57.29577951f;

      extern uint32 g_bitmasks[32];

      // Yes I know these should probably be pass by ref, not val:
      // http://www.stepanovpapers.com/notes.pdf
      // Just don't use them on non-simple (non built-in) types!
      template<typename T> inline T minimum(T a, T b)
      {
         return (a < b) ? a : b;
      }

      template<typename T> inline T minimum(T a, T b, T c)
      {
         return minimum(minimum(a, b), c);
      }

      template<typename T> inline T maximum(T a, T b)
      {
         return (a > b) ? a : b;
      }

      template<typename T> inline T maximum(T a, T b, T c)
      {
         return maximum(maximum(a, b), c);
      }

      template<typename T> inline T clamp(T value, T low, T high)
      {
         return (value < low) ? low : ((value > high) ? high : value);
      }

      template<typename T> inline T square(T value)
      {
         return value * value;
      }

      inline bool is_power_of_2(uint32 x)
      {
         return x && ((x & (x - 1U)) == 0U);
      }

      // From "Hackers Delight"
      inline int next_pow2(uint32 val)
      {
         val--;
         val |= val >> 16;
         val |= val >> 8;
         val |= val >> 4;
         val |= val >> 2;
         val |= val >> 1;
         return val + 1;
      }

      // Returns the total number of bits needed to encode v.
      inline uint32 total_bits(uint32 v)
      {
         uint32 l = 0;
         while (v > 0U)
         {
            v >>= 1;
            l++;
         }
         return l;
      }

      inline uint floor_log2i(uint v)
      {
         uint l = 0;
         while (v > 1U)
         {
            v >>= 1;
            l++;
         }
         return l;
      }

      inline uint ceil_log2i(uint v)
      {
         uint l = floor_log2i(v);
         if ((l != cIntBits) && (v > (1U << l)))
            l++;
         return l;
      }
   }
}

// File: crnd_utils.h
namespace crnd
{
   namespace utils
   {
      template<typename T> inline void zero_object(T& obj)
      {
         memset(&obj, 0, sizeof(obj));
      }

      template<typename T> inline void zero_this(T* pObj)
      {
         memset(pObj, 0, sizeof(*pObj));
      }

      template <typename T>
      inline void swap(T& left, T& right)
      {
         T temp(left);
         left = right;
         right = temp;
      }

      inline void invert_buf(void* pBuf, uint32 size)
      {
         uint8* p = static_cast<uint8*>(pBuf);

         const uint32 half_size = size >> 1;
         for (uint32 i = 0; i < half_size; i++)
            swap(p[i], p[size - 1U - i]);
      }

      static inline uint16 swap16(uint16 x) { return static_cast<uint16>((x << 8) | (x >> 8)); }
      static inline uint32 swap32(uint32 x) { return ((x << 24) | ((x << 8) & 0x00FF0000) | (( x >> 8) & 0x0000FF00) | (x >> 24)); }

      uint32 compute_max_mips(uint32 width, uint32 height);

   }   // namespace utils

} // namespace crnd

// File: crnd_vector.h
namespace crnd
{
   struct elemental_vector
   {
      void* m_p;
      uint32 m_size;
      uint32 m_capacity;

      typedef void (*object_mover)(void* pDst, void* pSrc, uint32 num);

      bool increase_capacity(uint32 min_new_capacity, bool grow_hint, uint32 element_size, object_mover pRelocate);
   };

#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable:4127) //  warning C4127: conditional expression is constant
#endif

   template<typename T>
   class vector : public helpers::rel_ops< vector<T> >
   {
   public:
      typedef T*              iterator;
      typedef const T*        const_iterator;
      typedef T               value_type;
      typedef T&              reference;
      typedef const T&        const_reference;
      typedef T*              pointer;
      typedef const T*        const_pointer;

      inline vector() :
         m_p(NULL),
         m_size(0),
         m_capacity(0),
         m_alloc_failed(false)
      {
      }

      inline vector(const vector& other) :
         m_p(NULL),
         m_size(0),
         m_capacity(0),
         m_alloc_failed(false)
      {
         *this = other;
      }

      inline vector(uint32 size) :
         m_p(NULL),
         m_size(0),
         m_capacity(0),
         m_alloc_failed(false)
      {
         resize(size);
      }

      inline ~vector()
      {
         clear();
      }

      // I don't like this. Not at all. But exceptions, or just failing suck worse.
      inline bool get_alloc_failed() const { return m_alloc_failed; }
      inline void clear_alloc_failed() { m_alloc_failed = false; }

      inline bool assign(const vector& other)
      {
         if (this == &other)
            return true;

         if (m_capacity == other.m_size)
            resize(0);
         else
         {
            clear();

            if (!increase_capacity(other.m_size, false))
               return false;
         }

         if (scalar_type<T>::cFlag)
            memcpy(m_p, other.m_p, other.m_size * sizeof(T));
         else
         {
            T* pDst = m_p;
            const T* pSrc = other.m_p;
            for (uint32 i = other.m_size; i > 0; i--)
               helpers::construct(pDst++, *pSrc++);
         }

         m_size = other.m_size;

         return true;
      }

      inline vector& operator= (const vector& other)
      {
         assign(other);
         return *this;
      }

      inline const   T* begin() const  { return m_p; }
      T* begin()        { return m_p; }

      inline const   T* end() const  { return m_p + m_size; }
      T* end()        { return m_p + m_size; }

      inline bool empty() const { return !m_size; }
      inline uint32 size() const { return m_size; }
      inline uint32 capacity() const { return m_capacity; }

      inline const T& operator[] (uint32 i) const  { CRND_ASSERT(i < m_size); return m_p[i]; }
      inline       T& operator[] (uint32 i)        { CRND_ASSERT(i < m_size); return m_p[i]; }

      inline const T& front() const  { CRND_ASSERT(m_size); return m_p[0]; }
      inline       T& front()        { CRND_ASSERT(m_size); return m_p[0]; }

      inline const T& back() const  { CRND_ASSERT(m_size); return m_p[m_size - 1]; }
      inline       T& back()        { CRND_ASSERT(m_size); return m_p[m_size - 1]; }

      inline void clear()
      {
         if (m_p)
         {
            scalar_type<T>::destruct_array(m_p, m_size);
            crnd_free(m_p);
            m_p = NULL;
            m_size = 0;
            m_capacity = 0;
         }

         m_alloc_failed = false;
      }

      inline bool reserve(uint32 new_capacity)
      {
         if (!increase_capacity(new_capacity, false))
            return false;

         return true;
      }

      inline bool resize(uint32 new_size)
      {
         if (m_size != new_size)
         {
            if (new_size < m_size)
               scalar_type<T>::destruct_array(m_p + new_size, m_size - new_size);
            else
            {
               if (new_size > m_capacity)
               {
                  if (!increase_capacity(new_size, new_size == (m_size + 1)))
                     return false;
               }

               scalar_type<T>::construct_array(m_p + m_size, new_size - m_size);
            }

            m_size = new_size;
         }

         return true;
      }

      inline bool push_back(const T& obj)
      {
         CRND_ASSERT(!m_p || (&obj < m_p) || (&obj >= (m_p + m_size)));

         if (m_size >= m_capacity)
         {
            if (!increase_capacity(m_size + 1, true))
               return false;
         }

         scalar_type<T>::construct(m_p + m_size, obj);
         m_size++;

         return true;
      }

      inline void pop_back()
      {
         CRND_ASSERT(m_size);

         if (m_size)
         {
            m_size--;
            scalar_type<T>::destruct(&m_p[m_size]);
         }
      }

      inline void insert(uint32 index, const T* p, uint32 n)
      {
         CRND_ASSERT(index <= m_size);
         if (!n)
            return;

         const uint32 orig_size = m_size;
         resize(m_size + n);

         const T* pSrc = m_p + orig_size - 1;
         T* pDst = const_cast<T*>(pSrc) + n;

         const uint32 num_to_move = orig_size - index;

         for (uint32 i = 0; i < num_to_move; i++)
         {
            CRND_ASSERT((pDst - m_p) < (int)m_size);
            *pDst-- = *pSrc--;
         }

         pSrc = p;
         pDst = m_p + index;

         for (uint32 i = 0; i < n; i++)
         {
            CRND_ASSERT((pDst - m_p) < (int)m_size);
            *pDst++ = *p++;
         }
      }

      inline void erase(uint32 start, uint32 n)
      {
         CRND_ASSERT((start + n) <= m_size);

         if (!n)
            return;

         const uint32 num_to_move = m_size - (start + n);

         T* pDst = m_p + start;
         T* pDst_end = pDst + num_to_move;
         const T* pSrc = m_p + start + n;

         while (pDst != pDst_end)
            *pDst++ = *pSrc++;

         scalar_type<T>::destruct_array(pDst_end, n);

         m_size -= n;
      }

      inline void erase(uint32 index)
      {
         erase(index, 1);
      }

      inline void erase(T* p)
      {
         CRND_ASSERT((p >= m_p) && (p < (m_p + m_size)));
         erase(p - m_p);
      }

      inline bool operator== (const vector& rhs) const
      {
         if (m_size != rhs.m_size)
            return false;
         else if (m_size)
         {
            if (scalar_type<T>::cFlag)
               return memcmp(m_p, rhs.m_p, sizeof(T) * m_size) == 0;
            else
            {
               const T* pSrc = m_p;
               const T* pDst = rhs.m_p;
               for (uint32 i = m_size; i; i--)
                  if (!(*pSrc++ == *pDst++))
                     return false;
            }
         }

         return true;
      }

      inline bool operator< (const vector& rhs) const
      {
         const uint32 min_size = math::minimum(m_size, rhs.m_size);

         const T* pSrc = m_p;
         const T* pSrc_end = m_p + min_size;
         const T* pDst = rhs.m_p;

         while ((pSrc < pSrc_end) && (*pSrc == *pDst))
         {
            pSrc++;
            pDst++;
         }

         if (pSrc < pSrc_end)
            return *pSrc < *pDst;

         return m_size < rhs.m_size;
      }

      void swap(vector& other)
      {
         utils::swap(m_p, other.m_p);
         utils::swap(m_size, other.m_size);
         utils::swap(m_capacity, other.m_capacity);
      }

   private:
      T*          m_p;
      uint32      m_size;
      uint32      m_capacity;
      bool        m_alloc_failed;

      template<typename Q> struct is_vector { enum { cFlag = false }; };
      template<typename Q> struct is_vector< vector<Q> > { enum { cFlag = true }; };

      static void object_mover(void* pDst_void, void* pSrc_void, uint32 num)
      {
         T* pSrc = static_cast<T*>(pSrc_void);
         T* const pSrc_end = pSrc + num;
         T* pDst = static_cast<T*>(pDst_void);

         while (pSrc != pSrc_end)
         {
            helpers::construct<T>(pDst, *pSrc);
            pSrc->~T();
            pSrc++;
            pDst++;
         }
      }

      inline bool increase_capacity(uint32 min_new_capacity, bool grow_hint)
      {
         if (!reinterpret_cast<elemental_vector*>(this)->increase_capacity(
            min_new_capacity, grow_hint, sizeof(T),
            ((scalar_type<T>::cFlag) || (is_vector<T>::cFlag) || (bitwise_movable<T>::cFlag) || CRND_IS_POD(T)) ? NULL : object_mover))
         {
            m_alloc_failed = true;
            return false;
         }
         return true;
      }
   };

#ifdef _MSC_VER
#pragma warning(pop)
#endif

   extern void vector_test();

} // namespace crnd

// File: crnd_private.h
namespace crnd
{
   const crn_header* crnd_get_header(crn_header& header, const void* pData, uint32 data_size);

} // namespace crnd

// File: checksum.h
namespace crnd
{
   // crc16() intended for small buffers - doesn't use an acceleration table.
   const uint16 cInitCRC16 = 0;
   uint16 crc16(const void* pBuf, uint32 len, uint16 crc = cInitCRC16);

}  // namespace crnd

// File: crnd_color.h
namespace crnd
{
   template<typename component_type> struct color_quad_component_traits
   {
      enum
      {
         cSigned = false,
         cFloat = false,
         cMin = cUINT8_MIN,
         cMax = cUINT8_MAX
      };
   };

   template<> struct color_quad_component_traits<int16>
   {
      enum
      {
         cSigned = true,
         cFloat = false,
         cMin = cINT16_MIN,
         cMax = cINT16_MAX
      };
   };

   template<> struct color_quad_component_traits<uint16>
   {
      enum
      {
         cSigned = false,
         cFloat = false,
         cMin = cUINT16_MIN,
         cMax = cUINT16_MAX
      };
   };

   template<> struct color_quad_component_traits<int32>
   {
      enum
      {
         cSigned = true,
         cFloat = false,
         cMin = cINT32_MIN,
         cMax = cINT32_MAX
      };
   };

   template<> struct color_quad_component_traits<uint32>
   {
      enum
      {
         cSigned = false,
         cFloat = false,
         cMin = cUINT32_MIN,
         cMax = cUINT32_MAX
      };
   };

   template<> struct color_quad_component_traits<float>
   {
      enum
      {
         cSigned = false,
         cFloat = true,
         cMin = cINT32_MIN,
         cMax = cINT32_MAX
      };
   };

   template<> struct color_quad_component_traits<double>
   {
      enum
      {
         cSigned = false,
         cFloat = true,
         cMin = cINT32_MIN,
         cMax = cINT32_MAX
      };
   };

#ifdef _MSC_VER
#pragma warning(push)
#pragma warning(disable:4201) //  warning C4201: nonstandard extension used : nameless struct/union
#pragma warning(disable:4127) //  warning C4127: conditional expression is constant
#endif

   template<typename component_type, typename parameter_type>
   class color_quad : public helpers::rel_ops<color_quad<component_type, parameter_type> >
   {
      static parameter_type clamp(parameter_type v)
      {
         if (component_traits::cFloat)
            return v;
         else
         {
            if (v < component_traits::cMin)
               return component_traits::cMin;
            else if (v > component_traits::cMax)
               return component_traits::cMax;
            return v;
         }
      }

   public:
      typedef component_type component_t;
      typedef parameter_type parameter_t;
      typedef color_quad_component_traits<component_type> component_traits;

      enum { cNumComps = 4 };

      union
      {
         struct
         {
            component_type r;
            component_type g;
            component_type b;
            component_type a;
         };

         component_type c[cNumComps];
      };

      inline color_quad()
      {
      }

      inline color_quad(eClear) :
         r(0), g(0), b(0), a(0)
      {
      }

      inline color_quad(const color_quad& other) :
         r(other.r), g(other.g), b(other.b), a(other.a)
      {
      }

      inline color_quad(parameter_type y, parameter_type alpha = component_traits::cMax)
      {
         set(y, alpha);
      }

      inline color_quad(parameter_type red, parameter_type green, parameter_type blue, parameter_type alpha = component_traits::cMax)
      {
         set(red, green, blue, alpha);
      }

      template<typename other_component_type, typename other_parameter_type>
      inline color_quad(const color_quad<other_component_type, other_parameter_type>& other) :
         r(clamp(other.r)), g(clamp(other.g)), b(clamp(other.b)), a(clamp(other.a))
      {
      }

      inline void clear()
      {
         r = 0;
         g = 0;
         b = 0;
         a = 0;
      }

      inline color_quad& operator= (const color_quad& other)
      {
         r = other.r;
         g = other.g;
         b = other.b;
         a = other.a;
         return *this;
      }

      template<typename other_component_type, typename other_parameter_type>
      inline color_quad& operator=(const color_quad<other_component_type, other_parameter_type>& other)
      {
         r = clamp(other.r);
         g = clamp(other.g);
         b = clamp(other.b);
         a = clamp(other.a);
         return *this;
      }

      inline color_quad& set(parameter_type y, parameter_type alpha = component_traits::cMax)
      {
         y = clamp(y);
         r = static_cast<component_type>(y);
         g = static_cast<component_type>(y);
         b = static_cast<component_type>(y);
         a = static_cast<component_type>(alpha);
         return *this;
      }

      inline color_quad& set(parameter_type red, parameter_type green, parameter_type blue, parameter_type alpha = component_traits::cMax)
      {
         r = static_cast<component_type>(clamp(red));
         g = static_cast<component_type>(clamp(green));
         b = static_cast<component_type>(clamp(blue));
         a = static_cast<component_type>(clamp(alpha));
         return *this;
      }

      inline color_quad& set_noclamp_rgba(parameter_type red, parameter_type green, parameter_type blue, parameter_type alpha)
      {
         r = static_cast<component_type>(red);
         g = static_cast<component_type>(green);
         b = static_cast<component_type>(blue);
         a = static_cast<component_type>(alpha);
         return *this;
      }

      inline color_quad& set_noclamp_rgb(parameter_type red, parameter_type green, parameter_type blue)
      {
         r = static_cast<component_type>(red);
         g = static_cast<component_type>(green);
         b = static_cast<component_type>(blue);
         return *this;
      }

      static inline parameter_type get_min_comp() { return component_traits::cMin; }
      static inline parameter_type get_max_comp() { return component_traits::cMax; }
      static inline bool get_comps_are_signed() { return component_traits::cSigned; }

      inline component_type operator[] (uint32 i) const { CRND_ASSERT(i < cNumComps); return c[i]; }
      inline component_type& operator[] (uint32 i) { CRND_ASSERT(i < cNumComps); return c[i]; }

      inline color_quad& set_component(uint32 i, parameter_type f)
      {
         CRND_ASSERT(i < cNumComps);

         c[i] = static_cast<component_type>(clamp(f));

         return *this;
      }

      inline color_quad& clamp(const color_quad& l, const color_quad& h)
      {
         for (uint32 i = 0; i < cNumComps; i++)
            c[i] = static_cast<component_type>(math::clamp<parameter_type>(c[i], l[i], h[i]));
         return *this;
      }

      inline color_quad& clamp(parameter_type l, parameter_type h)
      {
         for (uint32 i = 0; i < cNumComps; i++)
            c[i] = static_cast<component_type>(math::clamp<parameter_type>(c[i], l, h));
         return *this;
      }

      // Returns CCIR 601 luma (consistent with color_utils::RGB_To_Y).
      inline parameter_type get_luma() const
      {
         return static_cast<parameter_type>((19595U * r + 38470U * g + 7471U * b + 32768) >> 16U);
      }

      // Returns REC 709 luma.
      inline parameter_type get_luma_rec709() const
      {
         return static_cast<parameter_type>((13938U * r + 46869U * g + 4729U * b + 32768U) >> 16U);
      }

      inline uint32 squared_distance(const color_quad& c, bool alpha = true) const
      {
         return math::square(r - c.r) + math::square(g - c.g) + math::square(b - c.b) + (alpha ? math::square(a - c.a) : 0);
      }

      inline bool rgb_equals(const color_quad& rhs) const
      {
         return (r == rhs.r) && (g == rhs.g) && (b == rhs.b);
      }

      inline bool operator== (const color_quad& rhs) const
      {
         return (r == rhs.r) && (g == rhs.g) && (b == rhs.b) && (a == rhs.a);
      }

      inline bool operator< (const color_quad& rhs) const
      {
         for (uint32 i = 0; i < cNumComps; i++)
         {
            if (c[i] < rhs.c[i])
               return true;
            else if (!(c[i] == rhs.c[i]))
               return false;
         }
         return false;
      }

      inline color_quad& operator+= (const color_quad& other)
      {
         for (uint32 i = 0; i < 4; i++)
            c[i] = static_cast<component_type>(clamp(c[i] + other.c[i]));
         return *this;
      }

      inline color_quad& operator-= (const color_quad& other)
      {
         for (uint32 i = 0; i < 4; i++)
            c[i] = static_cast<component_type>(clamp(c[i] - other.c[i]));
         return *this;
      }

      inline color_quad& operator*= (parameter_type v)
      {
         for (uint32 i = 0; i < 4; i++)
            c[i] = static_cast<component_type>(clamp(c[i] * v));
         return *this;
      }

      inline color_quad& operator/= (parameter_type v)
      {
         for (uint32 i = 0; i < 4; i++)
            c[i] = static_cast<component_type>(c[i] / v);
         return *this;
      }

      inline color_quad get_swizzled(uint32 x, uint32 y, uint32 z, uint32 w) const
      {
         CRND_ASSERT((x | y | z | w) < 4);
         return color_quad(c[x], c[y], c[z], c[w]);
      }

      inline friend color_quad operator+ (const color_quad& lhs, const color_quad& rhs)
      {
         color_quad result(lhs);
         result += rhs;
         return result;
      }

      inline friend color_quad operator- (const color_quad& lhs, const color_quad& rhs)
      {
         color_quad result(lhs);
         result -= rhs;
         return result;
      }

      inline friend color_quad operator* (const color_quad& lhs, parameter_type v)
      {
         color_quad result(lhs);
         result *= v;
         return result;
      }

      friend inline color_quad operator/ (const color_quad& lhs, parameter_type v)
      {
         color_quad result(lhs);
         result /= v;
         return result;
      }

      friend inline color_quad operator* (parameter_type v, const color_quad& rhs)
      {
         color_quad result(rhs);
         result *= v;
         return result;
      }

      inline uint32 get_min_component_index(bool alpha = true) const
      {
         uint32 index = 0;
         uint32 limit = alpha ? cNumComps : (cNumComps - 1);
         for (uint32 i = 1; i < limit; i++)
            if (c[i] < c[index])
               index = i;
         return index;
      }

      inline uint32 get_max_component_index(bool alpha = true) const
      {
         uint32 index = 0;
         uint32 limit = alpha ? cNumComps : (cNumComps - 1);
         for (uint32 i = 1; i < limit; i++)
            if (c[i] > c[index])
               index = i;
         return index;
      }

      inline void get_float4(float* pDst)
      {
         for (uint32 i = 0; i < 4; i++)
            pDst[i] = ((*this)[i] - component_traits::cMin) / float(component_traits::cMax - component_traits::cMin);
      }

      inline void get_float3(float* pDst)
      {
         for (uint32 i = 0; i < 3; i++)
            pDst[i] = ((*this)[i] - component_traits::cMin) / float(component_traits::cMax - component_traits::cMin);
      }

      static inline color_quad make_black()
      {
         return color_quad(0, 0, 0, component_traits::cMax);
      }

      static inline color_quad make_white()
      {
         return color_quad(component_traits::cMax, component_traits::cMax, component_traits::cMax, component_traits::cMax);
      }
   }; // class color_quad

#ifdef _MSC_VER
#pragma warning(pop)
#endif

   template<typename c, typename q>
   struct scalar_type< color_quad<c, q> >
   {
      enum { cFlag = true };
      static inline void construct(color_quad<c, q>* p) { }
      static inline void construct(color_quad<c, q>* p, const color_quad<c, q>& init) { memcpy(p, &init, sizeof(color_quad<c, q>)); }
      static inline void construct_array(color_quad<c, q>* p, uint32 n) { p, n; }
      static inline void destruct(color_quad<c, q>* p) { p; }
      static inline void destruct_array(color_quad<c, q>* p, uint32 n) { p, n; }
   };

   typedef color_quad<uint8, int>      color_quad_u8;
   typedef color_quad<int16, int>      color_quad_i16;
   typedef color_quad<uint16, int>     color_quad_u16;
   typedef color_quad<int32, int>      color_quad_i32;
   typedef color_quad<uint32, uint32>    color_quad_u32;
   typedef color_quad<float, float>    color_quad_f;
   typedef color_quad<double, double>  color_quad_d;

} // namespace crnd

// File: crnd_dxt.h
namespace crnd
{
   enum dxt_format
   {
      cDXTInvalid = -1,

      // cDXT1/1A must appear first!
      cDXT1,
      cDXT1A,

      cDXT3,
      cDXT5,
      cDXT5A,

      cDXN_XY,    // inverted relative to standard ATI2, 360's DXN
      cDXN_YX     // standard ATI2
   };

   enum dxt_constants
   {
      cDXTBlockShift = 2U,
      cDXTBlockSize = 1U << cDXTBlockShift,

      cDXT1BytesPerBlock = 8U,
      cDXT5NBytesPerBlock = 16U,

      cDXT1SelectorBits = 2U,
      cDXT1SelectorValues = 1U << cDXT1SelectorBits,
      cDXT1SelectorMask = cDXT1SelectorValues - 1U,

      cDXT5SelectorBits = 3U,
      cDXT5SelectorValues = 1U << cDXT5SelectorBits,
      cDXT5SelectorMask = cDXT5SelectorValues - 1U
   };

   const float cDXT1MaxLinearValue = 3.0f;
   const float cDXT1InvMaxLinearValue = 1.0f/3.0f;

   const float cDXT5MaxLinearValue = 7.0f;
   const float cDXT5InvMaxLinearValue = 1.0f/7.0f;

   // Converts DXT1 raw color selector index to a linear value.
   extern const uint8 g_dxt1_to_linear[cDXT1SelectorValues];

   // Converts DXT5 raw alpha selector index to a linear value.
   extern const uint8 g_dxt5_to_linear[cDXT5SelectorValues];

   // Converts DXT1 linear color selector index to a raw value (inverse of g_dxt1_to_linear).
   extern const uint8 g_dxt1_from_linear[cDXT1SelectorValues];

   // Converts DXT5 linear alpha selector index to a raw value (inverse of g_dxt5_to_linear).
   extern const uint8 g_dxt5_from_linear[cDXT5SelectorValues];

   extern const uint8 g_six_alpha_invert_table[cDXT5SelectorValues];
   extern const uint8 g_eight_alpha_invert_table[cDXT5SelectorValues];

   struct dxt1_block
   {
      uint8 m_low_color[2];
      uint8 m_high_color[2];

      enum { cNumSelectorBytes = 4 };
      uint8 m_selectors[cNumSelectorBytes];

      inline void clear()
      {
         utils::zero_this(this);
      }

      // These methods assume the in-memory rep is in LE byte order.
      inline uint32 get_low_color() const
      {
         return m_low_color[0] | (m_low_color[1] << 8U);
      }

      inline uint32 get_high_color() const
      {
         return m_high_color[0] | (m_high_color[1] << 8U);
      }

      inline void set_low_color(uint16 c)
      {
         m_low_color[0] = static_cast<uint8>(c & 0xFF);
         m_low_color[1] = static_cast<uint8>((c >> 8) & 0xFF);
      }

      inline void set_high_color(uint16 c)
      {
         m_high_color[0] = static_cast<uint8>(c & 0xFF);
         m_high_color[1] = static_cast<uint8>((c >> 8) & 0xFF);
      }

      inline uint32 get_selector(uint32 x, uint32 y) const
      {
         CRND_ASSERT((x < 4U) && (y < 4U));
         return (m_selectors[y] >> (x * cDXT1SelectorBits)) & cDXT1SelectorMask;
      }

      inline void set_selector(uint32 x, uint32 y, uint32 val)
      {
         CRND_ASSERT((x < 4U) && (y < 4U) && (val < 4U));

         m_selectors[y] &= (~(cDXT1SelectorMask << (x * cDXT1SelectorBits)));
         m_selectors[y] |= (val << (x * cDXT1SelectorBits));
      }

      static uint16        pack_color(const color_quad_u8& color, bool scaled, uint32 bias = 127U);
      static uint16        pack_color(uint32 r, uint32 g, uint32 b, bool scaled, uint32 bias = 127U);

      static color_quad_u8 unpack_color(uint16 packed_color, bool scaled, uint32 alpha = 255U);
      static void          unpack_color(uint32& r, uint32& g, uint32& b, uint16 packed_color, bool scaled);

      static uint32        get_block_colors3(color_quad_u8* pDst, uint16 color0, uint16 color1);
      static uint32        get_block_colors4(color_quad_u8* pDst, uint16 color0, uint16 color1);
      // pDst must point to an array at least cDXT1SelectorValues long.
      static uint32        get_block_colors(color_quad_u8* pDst, uint16 color0, uint16 color1);

      static color_quad_u8 unpack_endpoint(uint32 endpoints, uint32 index, bool scaled, uint32 alpha = 255U);
      static uint32        pack_endpoints(uint32 lo, uint32 hi);
   };

   CRND_DEFINE_BITWISE_MOVABLE(dxt1_block);

   struct dxt3_block
   {
      enum { cNumAlphaBytes = 8 };
      uint8 m_alpha[cNumAlphaBytes];

      void set_alpha(uint32 x, uint32 y, uint32 value, bool scaled);
      uint32 get_alpha(uint32 x, uint32 y, bool scaled) const;
   };

   CRND_DEFINE_BITWISE_MOVABLE(dxt3_block);

   struct dxt5_block
   {
      uint8 m_endpoints[2];

      enum { cNumSelectorBytes = 6 };
      uint8 m_selectors[cNumSelectorBytes];

      inline void clear()
      {
         utils::zero_this(this);
      }

      inline uint32 get_low_alpha() const
      {
         return m_endpoints[0];
      }

      inline uint32 get_high_alpha() const
      {
         return m_endpoints[1];
      }

      inline void set_low_alpha(uint32 i)
      {
         CRND_ASSERT(i <= cUINT8_MAX);
         m_endpoints[0] = static_cast<uint8>(i);
      }

      inline void set_high_alpha(uint32 i)
      {
         CRND_ASSERT(i <= cUINT8_MAX);
         m_endpoints[1] = static_cast<uint8>(i);
      }

      uint32 get_endpoints_as_word() const { return m_endpoints[0] | (m_endpoints[1] << 8); }

      uint32 get_selectors_as_word(uint32 index) { CRND_ASSERT(index < 3); return m_selectors[index * 2] | (m_selectors[index * 2 + 1] << 8); }

      inline uint32 get_selector(uint32 x, uint32 y) const
      {
         CRND_ASSERT((x < 4U) && (y < 4U));

         uint32 selector_index = (y * 4) + x;
         uint32 bit_index = selector_index * cDXT5SelectorBits;

         uint32 byte_index = bit_index >> 3;
         uint32 bit_ofs = bit_index & 7;

         uint32 v = m_selectors[byte_index];
         if (byte_index < (cNumSelectorBytes - 1))
            v |= (m_selectors[byte_index + 1] << 8);

         return (v >> bit_ofs) & 7;
      }

      inline void set_selector(uint32 x, uint32 y, uint32 val)
      {
         CRND_ASSERT((x < 4U) && (y < 4U) && (val < 8U));

         uint32 selector_index = (y * 4) + x;
         uint32 bit_index = selector_index * cDXT5SelectorBits;

         uint32 byte_index = bit_index >> 3;
         uint32 bit_ofs = bit_index & 7;

         uint32 v = m_selectors[byte_index];
         if (byte_index < (cNumSelectorBytes - 1))
            v |= (m_selectors[byte_index + 1] << 8);

         v &= (~(7 << bit_ofs));
         v |= (val << bit_ofs);

         m_selectors[byte_index] = static_cast<uint8>(v);
         if (byte_index < (cNumSelectorBytes - 1))
            m_selectors[byte_index + 1] = static_cast<uint8>(v >> 8);
      }

      // Results written to alpha channel.
      static uint32          get_block_values6(color_quad_u8* pDst, uint32 l, uint32 h);
      static uint32          get_block_values8(color_quad_u8* pDst, uint32 l, uint32 h);
      static uint32          get_block_values(color_quad_u8* pDst, uint32 l, uint32 h);

      static uint32          get_block_values6(uint32* pDst, uint32 l, uint32 h);
      static uint32          get_block_values8(uint32* pDst, uint32 l, uint32 h);
      // pDst must point to an array at least cDXT5SelectorValues long.
      static uint32          get_block_values(uint32* pDst, uint32 l, uint32 h);

      static uint32          unpack_endpoint(uint32 packed, uint32 index);
      static uint32          pack_endpoints(uint32 lo, uint32 hi);
   };

   CRND_DEFINE_BITWISE_MOVABLE(dxt5_block);

} // namespace crnd

// File: crnd_dxt_hc_common.h
namespace crnd
{
   struct chunk_tile_desc
   {
      // These values are in pixels, and always a multiple of cBlockPixelWidth/cBlockPixelHeight.
      uint32 m_x_ofs;
      uint32 m_y_ofs;
      uint32 m_width;
      uint32 m_height;
      uint32 m_layout_index;
   };

   struct chunk_encoding_desc
   {
      uint32 m_num_tiles;
      chunk_tile_desc m_tiles[4];
   };

   const uint32 cChunkPixelWidth = 8;
   const uint32 cChunkPixelHeight = 8;
   const uint32 cChunkBlockWidth = 2;
   const uint32 cChunkBlockHeight = 2;

   const uint32 cChunkMaxTiles = 4;

   const uint32 cBlockPixelWidthShift = 2;
   const uint32 cBlockPixelHeightShift = 2;

   const uint32 cBlockPixelWidth = 4;
   const uint32 cBlockPixelHeight = 4;

   const uint32 cNumChunkEncodings = 8;
   extern chunk_encoding_desc g_chunk_encodings[cNumChunkEncodings];

   const uint32 cNumChunkTileLayouts = 9;
   const uint32 cFirst4x4ChunkTileLayout = 5;
   extern chunk_tile_desc g_chunk_tile_layouts[cNumChunkTileLayouts];

} // namespace crnd

// File: crnd_prefix_coding.h
#ifdef _XBOX
#define CRND_PREFIX_CODING_USE_FIXED_TABLE_SIZE 1
#else
#define CRND_PREFIX_CODING_USE_FIXED_TABLE_SIZE 0
#endif

namespace crnd
{
   namespace prefix_coding
   {
      const uint32 cMaxExpectedCodeSize = 16;
      const uint32 cMaxSupportedSyms = 8192;
      const uint32 cMaxTableBits = 11;

      class decoder_tables
      {
      public:
         inline decoder_tables() :
            m_cur_lookup_size(0), m_lookup(NULL), m_cur_sorted_symbol_order_size(0), m_sorted_symbol_order(NULL)
         {
         }

         inline decoder_tables(const decoder_tables& other) :
            m_cur_lookup_size(0), m_lookup(NULL), m_cur_sorted_symbol_order_size(0), m_sorted_symbol_order(NULL)
         {
            *this = other;
         }

         decoder_tables& operator= (const decoder_tables& other)
         {
            if (this == &other)
               return *this;

            clear();

            memcpy(this, &other, sizeof(*this));

            if (other.m_lookup)
            {
               m_lookup = crnd_new_array<uint32>(m_cur_lookup_size);
               if (m_lookup)
                  memcpy(m_lookup, other.m_lookup, sizeof(m_lookup[0]) * m_cur_lookup_size);
            }

            if (other.m_sorted_symbol_order)
            {
               m_sorted_symbol_order = crnd_new_array<uint16>(m_cur_sorted_symbol_order_size);
               if (m_sorted_symbol_order)
                  memcpy(m_sorted_symbol_order, other.m_sorted_symbol_order, sizeof(m_sorted_symbol_order[0]) * m_cur_sorted_symbol_order_size);
            }

            return *this;
         }

         inline void clear()
         {
            if (m_lookup)
            {
               crnd_delete_array(m_lookup);
               m_lookup = 0;
               m_cur_lookup_size = 0;
            }

            if (m_sorted_symbol_order)
            {
               crnd_delete_array(m_sorted_symbol_order);
               m_sorted_symbol_order = NULL;
               m_cur_sorted_symbol_order_size = 0;
            }
         }

         inline ~decoder_tables()
         {
            if (m_lookup)
               crnd_delete_array(m_lookup);

            if (m_sorted_symbol_order)
               crnd_delete_array(m_sorted_symbol_order);
         }

         bool init(uint32 num_syms, const uint8* pCodesizes, uint32 table_bits);

         // DO NOT use any complex classes here - it is bitwise copied.

         uint32                  m_num_syms;
         uint32                  m_total_used_syms;
         uint32                  m_table_bits;
         uint32                  m_table_shift;
         uint32                  m_table_max_code;
         uint32                  m_decode_start_code_size;

         uint8                   m_min_code_size;
         uint8                   m_max_code_size;

         uint32                  m_max_codes[cMaxExpectedCodeSize + 1];
         int32                   m_val_ptrs[cMaxExpectedCodeSize + 1];

         uint32                  m_cur_lookup_size;
         uint32*                 m_lookup;

         uint32                  m_cur_sorted_symbol_order_size;
         uint16*                 m_sorted_symbol_order;

         inline uint32 get_unshifted_max_code(uint32 len) const
         {
            CRND_ASSERT( (len >= 1) && (len <= cMaxExpectedCodeSize) );
            uint32 k = m_max_codes[len - 1];
            if (!k)
               return crnd::cUINT32_MAX;
            return (k - 1) >> (16 - len);
         }
      };

   } // namespace prefix_coding

} // namespace crnd

// File: crnd_symbol_codec.h
namespace crnd
{
   class static_huffman_data_model
   {
   public:
      static_huffman_data_model();
      static_huffman_data_model(const static_huffman_data_model& other);
      ~static_huffman_data_model();

      static_huffman_data_model& operator= (const static_huffman_data_model& rhs);

      bool init(uint32 total_syms, const uint8* pCode_sizes, uint32 code_size_limit);
      void clear();

      inline bool is_valid() const { return m_pDecode_tables != NULL; }

      inline uint32 get_total_syms() const { return m_total_syms; }

      inline uint32 get_code_size(uint32 sym) const { return m_code_sizes[sym]; }

      inline const uint8* get_code_sizes() const { return m_code_sizes.empty() ? NULL : &m_code_sizes[0]; }

   public:
      uint32                           m_total_syms;
      crnd::vector<uint8>              m_code_sizes;
      prefix_coding::decoder_tables*   m_pDecode_tables;

   private:
      bool prepare_decoder_tables();
      uint compute_decoder_table_bits() const;

      friend class symbol_codec;
   };

   class symbol_codec
   {
   public:
      symbol_codec();

      bool start_decoding(const uint8* pBuf, uint32 buf_size);
      bool decode_receive_static_data_model(static_huffman_data_model& model);

      uint32 decode_bits(uint32 num_bits);
      uint32 decode(const static_huffman_data_model& model);

      uint64 stop_decoding();

   public:
      const uint8*         m_pDecode_buf;
      const uint8*         m_pDecode_buf_next;
      const uint8*         m_pDecode_buf_end;
      uint32               m_decode_buf_size;

      typedef uint32 bit_buf_type;
      enum { cBitBufSize = 32U };
      bit_buf_type         m_bit_buf;

      int                  m_bit_count;

   private:
      void get_bits_init();
      uint32 get_bits(uint32 num_bits);
   };

} // namespace crnd

#define CRND_HUFF_DECODE_BEGIN(x)
#define CRND_HUFF_DECODE_END(x)
#define CRND_HUFF_DECODE(codec, model, symbol) symbol = codec.decode(model);

namespace crnd
{
   void crnd_assert(const char* pExp, const char* pFile, unsigned line)
   {
      char buf[512];

#if defined(_WIN32) && defined(_MSC_VER)
      sprintf_s(buf, sizeof(buf), "%s(%u): Assertion failure: \"%s\"\n", pFile, line, pExp);
#else
      sprintf(buf, "%s(%u): Assertion failure: \"%s\"\n", pFile, line, pExp);
#endif

      crnd_output_debug_string(buf);

      puts(buf);

      if (crnd_is_debugger_present())
         crnd_debug_break();
   }

   void crnd_trace(const char* pFmt, va_list args)
   {
      if (crnd_is_debugger_present())
      {
         char buf[512];
#if defined(_WIN32) && defined(_MSC_VER)
         vsprintf_s(buf, sizeof(buf), pFmt, args);
#else
         vsprintf(buf, pFmt, args);
#endif

         crnd_output_debug_string(buf);
      }
   };

   void crnd_trace(const char* pFmt, ...)
   {
      va_list args;
      va_start(args, pFmt);
      crnd_trace(pFmt, args);
      va_end(args);
   };

} // namespace crnd

// File: checksum.cpp
// From the public domain stb.h header.
namespace crnd
{
   uint16 crc16(const void* pBuf, uint32 len, uint16 crc)
   {
      crc = ~crc;

      const uint8* p = reinterpret_cast<const uint8*>(pBuf);
      while (len)
      {
         const uint16 q = *p++ ^ (crc >> 8U);
         crc <<= 8U;

         uint16 r = (q >> 4U) ^ q;
         crc ^= r;
         r <<= 5U;
         crc ^= r;
         r <<= 7U;
         crc ^= r;

         len--;
      }

      return static_cast<uint16>(~crc);
   }

} // namespace crnd


// File: crnd_vector.cpp
namespace crnd
{
   bool elemental_vector::increase_capacity(uint32 min_new_capacity, bool grow_hint, uint32 element_size, object_mover pMover)
   {
      CRND_ASSERT(m_size <= m_capacity);
      CRND_ASSERT(min_new_capacity < (0x7FFF0000U / element_size));

      if (m_capacity >= min_new_capacity)
         return true;

      uint32 new_capacity = min_new_capacity;
      if ((grow_hint) && (!math::is_power_of_2(new_capacity)))
         new_capacity = math::next_pow2(new_capacity);

      CRND_ASSERT(new_capacity && (new_capacity > m_capacity));

      const uint32 desired_size = element_size * new_capacity;
      size_t actual_size;
      if (!pMover)
      {
         void* new_p = crnd_realloc(m_p, desired_size, &actual_size, true);
         if (!new_p)
            return false;
         m_p = new_p;
      }
      else
      {
         void* new_p = crnd_malloc(desired_size, &actual_size);
         if (!new_p)
            return false;

         (*pMover)(new_p, m_p, m_size);

         if (m_p)
            crnd_free(m_p);

         m_p = new_p;
      }

      if (actual_size > desired_size)
         m_capacity = static_cast<uint32>(actual_size / element_size);
      else
         m_capacity = new_capacity;

      return true;
   }

} // namespace crnd

// File: crnd_utils.cpp
namespace crnd
{
   namespace utils
   {
      uint32 compute_max_mips(uint32 width, uint32 height)
      {
         if ((width | height) == 0)
            return 0;

         uint32 num_mips = 1;

         while ((width > 1U) || (height > 1U))
         {
            width >>= 1U;
            height >>= 1U;
            num_mips++;
         }

         return num_mips;
      }

   } // namespace utils

} // namespace crnd

// File: crnd_prefix_coding.cpp
namespace crnd
{
   namespace prefix_coding
   {
      bool decoder_tables::init(uint32 num_syms, const uint8* pCodesizes, uint32 table_bits)
      {
         uint32 min_codes[cMaxExpectedCodeSize];
         if ((!num_syms) || (table_bits > cMaxTableBits))
            return false;

         m_num_syms = num_syms;

         uint32 num_codes[cMaxExpectedCodeSize + 1];
         utils::zero_object(num_codes);

         for (uint32 i = 0; i < num_syms; i++)
         {
            uint32 c = pCodesizes[i];
            if (c)
               num_codes[c]++;
         }

         uint32 sorted_positions[cMaxExpectedCodeSize + 1];

         uint32 cur_code = 0;

         uint32 total_used_syms = 0;
         uint32 max_code_size = 0;
         uint32 min_code_size = cUINT32_MAX;
         for (uint32 i = 1; i <= cMaxExpectedCodeSize; i++)
         {
            const uint32 n = num_codes[i];

            if (!n)
               m_max_codes[i - 1] = 0;//UINT_MAX;
            else
            {
               min_code_size = math::minimum(min_code_size, i);
               max_code_size = math::maximum(max_code_size, i);

               min_codes[i - 1] = cur_code;

               m_max_codes[i - 1] = cur_code + n - 1;
               m_max_codes[i - 1] = 1 + ((m_max_codes[i - 1] << (16 - i)) | ((1 << (16 - i)) - 1));

               m_val_ptrs[i - 1] = total_used_syms;

               sorted_positions[i] = total_used_syms;

               cur_code += n;
               total_used_syms += n;
            }

            cur_code <<= 1;
         }

         m_total_used_syms = total_used_syms;

         if (total_used_syms > m_cur_sorted_symbol_order_size)
         {
            m_cur_sorted_symbol_order_size = total_used_syms;

            if (!math::is_power_of_2(total_used_syms))
               m_cur_sorted_symbol_order_size = math::minimum<uint32>(num_syms, math::next_pow2(total_used_syms));

            if (m_sorted_symbol_order)
               crnd_delete_array(m_sorted_symbol_order);

            m_sorted_symbol_order = crnd_new_array<uint16>(m_cur_sorted_symbol_order_size);
            if (!m_sorted_symbol_order)
               return false;
         }

         m_min_code_size = static_cast<uint8>(min_code_size);
         m_max_code_size = static_cast<uint8>(max_code_size);

         for (uint32 i = 0; i < num_syms; i++)
         {
            uint32 c = pCodesizes[i];
            if (c)
            {
               CRND_ASSERT(num_codes[c]);

               uint32 sorted_pos = sorted_positions[c]++;

               CRND_ASSERT(sorted_pos < total_used_syms);

               m_sorted_symbol_order[sorted_pos] = static_cast<uint16>(i);
            }
         }

         if (table_bits <= m_min_code_size)
            table_bits = 0;
         m_table_bits = table_bits;

         if (table_bits)
         {
            uint32 table_size = 1 << table_bits;
            if (table_size > m_cur_lookup_size)
            {
               m_cur_lookup_size = table_size;

               if (m_lookup)
                  crnd_delete_array(m_lookup);

               m_lookup = crnd_new_array<uint32>(table_size);
               if (!m_lookup)
                  return false;
            }

            memset(m_lookup, 0xFF, (uint)sizeof(m_lookup[0]) * (1UL << table_bits));

            for (uint32 codesize = 1; codesize <= table_bits; codesize++)
            {
               if (!num_codes[codesize])
                  continue;

               const uint32 fillsize = table_bits - codesize;
               const uint32 fillnum = 1 << fillsize;

               const uint32 min_code = min_codes[codesize - 1];
               const uint32 max_code = get_unshifted_max_code(codesize);
               const uint32 val_ptr = m_val_ptrs[codesize - 1];

               for (uint32 code = min_code; code <= max_code; code++)
               {
                  const uint32 sym_index = m_sorted_symbol_order[ val_ptr + code - min_code ];
                  CRND_ASSERT( pCodesizes[sym_index] == codesize );

                  for (uint32 j = 0; j < fillnum; j++)
                  {
                     const uint32 t = j + (code << fillsize);

                     CRND_ASSERT(t < (1U << table_bits));

                     CRND_ASSERT(m_lookup[t] == cUINT32_MAX);

                     m_lookup[t] = sym_index | (codesize << 16U);
                  }
               }
            }
         }

         for (uint32 i = 0; i < cMaxExpectedCodeSize; i++)
            m_val_ptrs[i] -= min_codes[i];

         m_table_max_code = 0;
         m_decode_start_code_size = m_min_code_size;

         if (table_bits)
         {
            uint32 i;
            for (i = table_bits; i >= 1; i--)
            {
               if (num_codes[i])
               {
                  m_table_max_code = m_max_codes[i - 1];
                  break;
               }
            }
            if (i >= 1)
            {
               m_decode_start_code_size = table_bits + 1;
               for (uint32 j = table_bits + 1; j <= max_code_size; j++)
               {
                  if (num_codes[j])
                  {
                     m_decode_start_code_size = j;
                     break;
                  }
               }
            }
         }

         // sentinels
         m_max_codes[cMaxExpectedCodeSize] = cUINT32_MAX;
         m_val_ptrs[cMaxExpectedCodeSize] = 0xFFFFF;

         m_table_shift = 32 - m_table_bits;
         return true;
      }

   } // namespace prefix_codig

} // namespace crnd

// File: crnd_platform.cpp
namespace crnd
{
   bool crnd_is_debugger_present()
   {
#ifdef CRND_DEVEL
      return IsDebuggerPresent() != 0;
#else
      return false;
#endif
   }

   void crnd_debug_break()
   {
#ifdef CRND_DEVEL
      DebugBreak();
#endif
   }

   void crnd_output_debug_string(const char* p)
   {
      p;
#ifdef CRND_DEVEL
      OutputDebugStringA(p);
#endif
   }

} // namespace crnd

// File: crnd_mem.cpp
namespace crnd
{
   const uint32 MAX_POSSIBLE_BLOCK_SIZE = 0x7FFF0000U;

   static void* crnd_default_realloc(void* p, size_t size, size_t* pActual_size, bool movable, void* pUser_data)
   {
      pUser_data;

      void* p_new;

      if (!p)
      {
         p_new = ::malloc(size);

         if (pActual_size)
         {
#ifdef _WIN32
            *pActual_size = p_new ? ::_msize(p_new) : 0;
#else
            *pActual_size = p_new ? malloc_usable_size(p_new) : 0;
#endif
         }
      }
      else if (!size)
      {
         ::free(p);
         p_new = NULL;

         if (pActual_size)
            *pActual_size = 0;
      }
      else
      {
         void* p_final_block = p;
#ifdef _WIN32
         p_new = ::_expand(p, size);
#else
         p_new = NULL;
#endif

         if (p_new)
            p_final_block = p_new;
         else if (movable)
         {
            p_new = ::realloc(p, size);

            if (p_new)
               p_final_block = p_new;
         }

         if (pActual_size)
         {
#ifdef _WIN32
            *pActual_size = ::_msize(p_final_block);
#else
            *pActual_size = ::malloc_usable_size(p_final_block);
#endif
         }
      }

      return p_new;
   }

   static size_t crnd_default_msize(void* p, void* pUser_data)
   {
      pUser_data;
#ifdef _WIN32
      return p ? _msize(p) : 0;
#else
      return p ? malloc_usable_size(p) : 0;
#endif
   }

   static crnd_realloc_func        g_pRealloc = crnd_default_realloc;
   static crnd_msize_func          g_pMSize   = crnd_default_msize;
   static void*                   g_pUser_data;

   void crnd_set_memory_callbacks(crnd_realloc_func pRealloc, crnd_msize_func pMSize, void* pUser_data)
   {
      if ((!pRealloc) || (!pMSize))
      {
         g_pRealloc = crnd_default_realloc;
         g_pMSize = crnd_default_msize;
         g_pUser_data = NULL;
      }
      else
      {
         g_pRealloc = pRealloc;
         g_pMSize = pMSize;
         g_pUser_data = pUser_data;
      }
   }

   static inline void crnd_mem_error(const char* p_msg)
   {
      crnd_assert(p_msg, __FILE__, __LINE__);
   }

   void* crnd_malloc(size_t size, size_t* pActual_size)
   {
      size = (size + sizeof(uint32) - 1U) & ~(sizeof(uint32) - 1U);
      if (!size)
         size = sizeof(uint32);

      if (size > MAX_POSSIBLE_BLOCK_SIZE)
      {
         crnd_mem_error("crnd_malloc: size too big");
         return NULL;
      }

      size_t actual_size = size;
      uint8* p_new = static_cast<uint8*>((*g_pRealloc)(NULL, size, &actual_size, true, g_pUser_data));

      if (pActual_size)
         *pActual_size = actual_size;

      if ((!p_new) || (actual_size < size))
      {
         crnd_mem_error("crnd_malloc: out of memory");
         return NULL;
      }

      CRND_ASSERT(((uint32)p_new & (CRND_MIN_ALLOC_ALIGNMENT - 1)) == 0);

      return p_new;
   }

   void* crnd_realloc(void* p, size_t size, size_t* pActual_size, bool movable)
   {
      if ((uint32)reinterpret_cast<ptr_bits>(p) & (CRND_MIN_ALLOC_ALIGNMENT - 1))
      {
         crnd_mem_error("crnd_realloc: bad ptr");
         return NULL;
      }

      if (size > MAX_POSSIBLE_BLOCK_SIZE)
      {
         crnd_mem_error("crnd_malloc: size too big");
         return NULL;
      }

      size_t actual_size = size;
      void* p_new = (*g_pRealloc)(p, size, &actual_size, movable, g_pUser_data);

      if (pActual_size)
         *pActual_size = actual_size;

      CRND_ASSERT(((uint32)p_new & (CRND_MIN_ALLOC_ALIGNMENT - 1)) == 0);

      return p_new;
   }

   void crnd_free(void* p)
   {
      if (!p)
         return;

      if ((uint32)reinterpret_cast<ptr_bits>(p) & (CRND_MIN_ALLOC_ALIGNMENT - 1))
      {
         crnd_mem_error("crnd_free: bad ptr");
         return;
      }

      (*g_pRealloc)(p, 0, NULL, true, g_pUser_data);
   }

   size_t crnd_msize(void* p)
   {
      if (!p)
         return 0;

      if ((uint32)reinterpret_cast<ptr_bits>(p) & (CRND_MIN_ALLOC_ALIGNMENT - 1))
      {
         crnd_mem_error("crnd_msize: bad ptr");
         return 0;
      }

      return (*g_pMSize)(p, g_pUser_data);
   }

} // namespace crnd

// File: crnd_math.cpp
namespace crnd
{
   namespace math
   {
      uint32 g_bitmasks[32] =
      {
         1U <<  0U,         1U <<  1U,          1U <<  2U,        1U <<  3U,
         1U <<  4U,         1U <<  5U,          1U <<  6U,        1U <<  7U,
         1U <<  8U,         1U <<  9U,          1U << 10U,        1U << 11U,
         1U << 12U,         1U << 13U,          1U << 14U,        1U << 15U,
         1U << 16U,         1U << 17U,          1U << 18U,        1U << 19U,
         1U << 20U,         1U << 21U,          1U << 22U,        1U << 23U,
         1U << 24U,         1U << 25U,          1U << 26U,        1U << 27U,
         1U << 28U,         1U << 29U,          1U << 30U,        1U << 31U
      };

   } // namespace math
} // namespace crnd

// File: crnd_info.cpp
namespace crnd
{
#define CRND_FOURCC(a, b, c, d) ((a) | ((b) << 8U) | ((c) << 16U) | ((d) << 24U))

   uint32 crnd_crn_format_to_fourcc(crn_format fmt)
   {
      switch (fmt)
      {
         case cCRNFmtDXT1:        return CRND_FOURCC('D', 'X', 'T', '1');
         case cCRNFmtDXT3:        return CRND_FOURCC('D', 'X', 'T', '3');
         case cCRNFmtDXT5:        return CRND_FOURCC('D', 'X', 'T', '5');
         case cCRNFmtDXN_XY:      return CRND_FOURCC('A', '2', 'X', 'Y');
         case cCRNFmtDXN_YX:      return CRND_FOURCC('A', 'T', 'I', '2');
         case cCRNFmtDXT5A:       return CRND_FOURCC('A', 'T', 'I', '1');
         case cCRNFmtDXT5_CCxY:   return CRND_FOURCC('C', 'C', 'x', 'Y');
         case cCRNFmtDXT5_xGxR:   return CRND_FOURCC('x', 'G', 'x', 'R');
         case cCRNFmtDXT5_xGBR:   return CRND_FOURCC('x', 'G', 'B', 'R');
         case cCRNFmtDXT5_AGBR:   return CRND_FOURCC('A', 'G', 'B', 'R');
         case cCRNFmtETC1:        return CRND_FOURCC('E', 'T', 'C', '1');
         default: break;
      }
      CRND_ASSERT(false);
      return 0;
   }

   crn_format crnd_get_fundamental_dxt_format(crn_format fmt)
   {
      switch (fmt)
      {
         case cCRNFmtDXT5_CCxY:
         case cCRNFmtDXT5_xGxR:
         case cCRNFmtDXT5_xGBR:
         case cCRNFmtDXT5_AGBR:
            return cCRNFmtDXT5;
         default: break;
      }
      return fmt;
   }

   uint32 crnd_get_crn_format_bits_per_texel(crn_format fmt)
   {
      switch (fmt)
      {
         case cCRNFmtDXT1:
         case cCRNFmtDXT5A:
         case cCRNFmtETC1:
            return 4;
         case cCRNFmtDXT3:
         case cCRNFmtDXT5:
         case cCRNFmtDXN_XY:
         case cCRNFmtDXN_YX:
         case cCRNFmtDXT5_CCxY:
         case cCRNFmtDXT5_xGxR:
         case cCRNFmtDXT5_xGBR:
         case cCRNFmtDXT5_AGBR:
            return 8;
         default: break;
      }
      CRND_ASSERT(false);
      return 0;
   }

   uint32 crnd_get_bytes_per_dxt_block(crn_format fmt)
   {
      return (crnd_get_crn_format_bits_per_texel(fmt) << 4) >> 3;
   }

   // TODO: tmp_header isn't used/This function is a helper to support old headers.
   const crn_header* crnd_get_header(crn_header& tmp_header, const void* pData, uint32 data_size)
   {
      tmp_header;

      if ((!pData) || (data_size < sizeof(crn_header)))
         return NULL;

      const crn_header& file_header = *static_cast<const crn_header*>(pData);
      if (file_header.m_sig != crn_header::cCRNSigValue)
         return NULL;

      if ((file_header.m_header_size < sizeof(crn_header)) || (data_size < file_header.m_data_size))
         return NULL;

      return &file_header;
   }

   bool crnd_validate_file(const void* pData, uint32 data_size, crn_file_info* pFile_info)
   {
      if (pFile_info)
      {
         if (pFile_info->m_struct_size != sizeof(crn_file_info))
            return false;

         memset(&pFile_info->m_struct_size + 1, 0, sizeof(crn_file_info) - sizeof(pFile_info->m_struct_size));
      }

      if ((!pData) || (data_size < cCRNHeaderMinSize))
         return false;

      crn_header tmp_header;
      const crn_header* pHeader = crnd_get_header(tmp_header, pData, data_size);
      if (!pHeader)
         return false;

      const uint32 header_crc = crc16(&pHeader->m_data_size, (uint32)(pHeader->m_header_size - ((const uint8*)&pHeader->m_data_size - (const uint8*)pHeader)));
      if (header_crc != pHeader->m_header_crc16)
         return false;

      const uint32 data_crc = crc16((const uint8*)pData + pHeader->m_header_size, pHeader->m_data_size - pHeader->m_header_size);
      if (data_crc != pHeader->m_data_crc16)
         return false;

      if ((pHeader->m_faces != 1) && (pHeader->m_faces != 6))
         return false;
      if ((pHeader->m_width < 1) || (pHeader->m_width > cCRNMaxLevelResolution))
         return false;
      if ((pHeader->m_height < 1) || (pHeader->m_height > cCRNMaxLevelResolution))
         return false;
      if ((pHeader->m_levels < 1) || (pHeader->m_levels > utils::compute_max_mips(pHeader->m_width, pHeader->m_height)))
         return false;
      if (((int)pHeader->m_format < cCRNFmtDXT1) || ((int)pHeader->m_format >= cCRNFmtTotal))
         return false;

      if (pFile_info)
      {
         pFile_info->m_actual_data_size = pHeader->m_data_size;
         pFile_info->m_header_size = pHeader->m_header_size;
         pFile_info->m_total_palette_size = pHeader->m_color_endpoints.m_size + pHeader->m_color_selectors.m_size + pHeader->m_alpha_endpoints.m_size + pHeader->m_alpha_selectors.m_size;
         pFile_info->m_tables_size = pHeader->m_tables_size;

         pFile_info->m_levels = pHeader->m_levels;

         for (uint32 i = 0; i < pHeader->m_levels; i++)
         {
            uint32 next_ofs = pHeader->m_data_size;

            // assumes the levels are packed together sequentially
            if ((i + 1) < pHeader->m_levels)
               next_ofs = pHeader->m_level_ofs[i + 1];

            pFile_info->m_level_compressed_size[i] = next_ofs - pHeader->m_level_ofs[i];
         }

         pFile_info->m_color_endpoint_palette_entries = pHeader->m_color_endpoints.m_num;
         pFile_info->m_color_selector_palette_entries = pHeader->m_color_selectors.m_num;;
         pFile_info->m_alpha_endpoint_palette_entries = pHeader->m_alpha_endpoints.m_num;;
         pFile_info->m_alpha_selector_palette_entries = pHeader->m_alpha_selectors.m_num;;
      }

      return true;
   }

   bool crnd_get_texture_info(const void* pData, uint32 data_size, crn_texture_info* pInfo)
   {
      if ((!pData) || (data_size < sizeof(crn_header)) || (!pInfo))
         return false;

      if (pInfo->m_struct_size != sizeof(crn_texture_info))
         return false;

      crn_header tmp_header;
      const crn_header* pHeader = crnd_get_header(tmp_header, pData, data_size);
      if (!pHeader)
         return false;

      pInfo->m_width = pHeader->m_width;
      pInfo->m_height = pHeader->m_height;
      pInfo->m_levels = pHeader->m_levels;
      pInfo->m_faces = pHeader->m_faces;
      pInfo->m_format = static_cast<crn_format>((uint32)pHeader->m_format);
      pInfo->m_bytes_per_block = ((pHeader->m_format == cCRNFmtDXT1) || (pHeader->m_format == cCRNFmtDXT5A)) ? 8 : 16;
      pInfo->m_userdata0 = pHeader->m_userdata0;
      pInfo->m_userdata1 = pHeader->m_userdata1;

      return true;
   }

   bool crnd_get_level_info(const void* pData, uint32 data_size, uint32 level_index, crn_level_info* pLevel_info)
   {
      if ((!pData) || (data_size < cCRNHeaderMinSize) || (!pLevel_info))
         return false;

      if (pLevel_info->m_struct_size != sizeof(crn_level_info))
         return false;

      crn_header tmp_header;
      const crn_header* pHeader = crnd_get_header(tmp_header, pData, data_size);
      if (!pHeader)
         return false;

      if (level_index >= pHeader->m_levels)
         return false;

      uint32 width = math::maximum<uint32>(1U, pHeader->m_width >> level_index);
      uint32 height = math::maximum<uint32>(1U, pHeader->m_height >> level_index);

      pLevel_info->m_width = width;
      pLevel_info->m_height = height;
      pLevel_info->m_faces = pHeader->m_faces;
      pLevel_info->m_blocks_x = (width + 3) >> 2;
      pLevel_info->m_blocks_y = (height + 3) >> 2;
      pLevel_info->m_bytes_per_block = ((pHeader->m_format == cCRNFmtDXT1) || (pHeader->m_format == cCRNFmtDXT5A)) ? 8 : 16;
      pLevel_info->m_format = static_cast<crn_format>((uint32)pHeader->m_format);

      return true;
   }

   const void* crnd_get_level_data(const void* pData, uint32 data_size, uint32 level_index, uint32* pSize)
   {
      if (pSize)
         *pSize = 0;

      if ((!pData) || (data_size < cCRNHeaderMinSize))
         return NULL;

      crn_header tmp_header;
      const crn_header* pHeader = crnd_get_header(tmp_header, pData, data_size);
      if (!pHeader)
         return NULL;

      if (level_index >= pHeader->m_levels)
         return NULL;

      uint32 cur_level_ofs = pHeader->m_level_ofs[level_index];

      if (pSize)
      {
         uint32 next_level_ofs = data_size;
         if ((level_index + 1) < (pHeader->m_levels))
            next_level_ofs = pHeader->m_level_ofs[level_index + 1];

         *pSize = next_level_ofs - cur_level_ofs;
      }

      return static_cast<const uint8*>(pData) + cur_level_ofs;
   }

   uint32 crnd_get_segmented_file_size(const void* pData, uint32 data_size)
   {
      if ((!pData) || (data_size < cCRNHeaderMinSize))
         return false;

      crn_header tmp_header;
      const crn_header* pHeader = crnd_get_header(tmp_header, pData, data_size);
      if (!pHeader)
         return false;

      uint32 size = pHeader->m_header_size;

      size = math::maximum(size, pHeader->m_color_endpoints.m_ofs + pHeader->m_color_endpoints.m_size);
      size = math::maximum(size, pHeader->m_color_selectors.m_ofs + pHeader->m_color_selectors.m_size);
      size = math::maximum(size, pHeader->m_alpha_endpoints.m_ofs + pHeader->m_alpha_endpoints.m_size);
      size = math::maximum(size, pHeader->m_alpha_selectors.m_ofs + pHeader->m_alpha_selectors.m_size);
      size = math::maximum(size, pHeader->m_tables_ofs + pHeader->m_tables_size);

      return size;
   }

   bool crnd_create_segmented_file(const void* pData, uint32 data_size, void* pBase_data, uint base_data_size)
   {
      if ((!pData) || (data_size < cCRNHeaderMinSize))
         return false;

      crn_header tmp_header;
      const crn_header* pHeader = crnd_get_header(tmp_header, pData, data_size);
      if (!pHeader)
         return false;

      if (pHeader->m_flags & cCRNHeaderFlagSegmented)
         return false;

      const uint actual_base_data_size = crnd_get_segmented_file_size(pData, data_size);
      if (base_data_size < actual_base_data_size)
         return false;

      memcpy(pBase_data, pData, actual_base_data_size);

      crn_header& new_header = *static_cast<crn_header*>(pBase_data);
      new_header.m_flags = new_header.m_flags | cCRNHeaderFlagSegmented;
      new_header.m_data_size = actual_base_data_size;

      new_header.m_data_crc16 = crc16((const uint8*)pBase_data + new_header.m_header_size, new_header.m_data_size - new_header.m_header_size);

      new_header.m_header_crc16 = crc16(&new_header.m_data_size, new_header.m_header_size - (uint32)((const uint8*)&new_header.m_data_size - (const uint8*)&new_header));

      CRND_ASSERT(crnd_validate_file(&new_header, actual_base_data_size, NULL));

      return true;
   }

} // namespace crnd

// File: symbol_codec.cpp
namespace crnd
{
   static_huffman_data_model::static_huffman_data_model() :
m_total_syms(0),
m_pDecode_tables(NULL)
{
}

static_huffman_data_model::static_huffman_data_model(const static_huffman_data_model& other) :
m_total_syms(0),
m_pDecode_tables(NULL)
{
   *this = other;
}

static_huffman_data_model::~static_huffman_data_model()
{
   if (m_pDecode_tables)
      crnd_delete(m_pDecode_tables);
}

static_huffman_data_model& static_huffman_data_model::operator=(const static_huffman_data_model& rhs)
{
   if (this == &rhs)
      return *this;

   m_total_syms = rhs.m_total_syms;
   m_code_sizes = rhs.m_code_sizes;
   if (m_code_sizes.get_alloc_failed())
   {
      clear();
      return *this;
   }

   if (rhs.m_pDecode_tables)
   {
      if (m_pDecode_tables)
         *m_pDecode_tables = *rhs.m_pDecode_tables;
      else
         m_pDecode_tables = crnd_new<prefix_coding::decoder_tables>(*rhs.m_pDecode_tables);
   }
   else
   {
      crnd_delete(m_pDecode_tables);
      m_pDecode_tables = NULL;
   }

   return *this;
}

void static_huffman_data_model::clear()
{
   m_total_syms = 0;
   m_code_sizes.clear();
   if (m_pDecode_tables)
   {
      crnd_delete(m_pDecode_tables);
      m_pDecode_tables = NULL;
   }
}

bool static_huffman_data_model::init(uint32 total_syms, const uint8* pCode_sizes, uint32 code_size_limit)
{
   CRND_ASSERT((total_syms >= 1) && (total_syms <= prefix_coding::cMaxSupportedSyms) && (code_size_limit >= 1));

   code_size_limit = math::minimum(code_size_limit, prefix_coding::cMaxExpectedCodeSize);

   if (!m_code_sizes.resize(total_syms))
      return false;

   uint32 min_code_size = cUINT32_MAX;
   uint32 max_code_size = 0;

   for (uint32 i = 0; i < total_syms; i++)
   {
      uint32 s = pCode_sizes[i];
      m_code_sizes[i] = static_cast<uint8>(s);
      min_code_size = math::minimum(min_code_size, s);
      max_code_size = math::maximum(max_code_size, s);
   }

   if ((max_code_size < 1) || (max_code_size > 32) || (min_code_size > code_size_limit))
      return false;

   if (max_code_size > code_size_limit)
      return false;

   if (!m_pDecode_tables)
      m_pDecode_tables = crnd_new<prefix_coding::decoder_tables>();

   if (!m_pDecode_tables->init(m_total_syms, &m_code_sizes[0], compute_decoder_table_bits()))
      return false;

   return true;
}

bool static_huffman_data_model::prepare_decoder_tables()
{
   uint32 total_syms = m_code_sizes.size();

   CRND_ASSERT((total_syms >= 1) && (total_syms <= prefix_coding::cMaxSupportedSyms));

   m_total_syms = total_syms;

   if (!m_pDecode_tables)
      m_pDecode_tables = crnd_new<prefix_coding::decoder_tables>();

   return m_pDecode_tables->init(m_total_syms, &m_code_sizes[0], compute_decoder_table_bits());
}

uint static_huffman_data_model::compute_decoder_table_bits() const
{
#if CRND_PREFIX_CODING_USE_FIXED_TABLE_SIZE
   return prefix_coding::cMaxTableBits;
#else
   uint32 decoder_table_bits = 0;
   if (m_total_syms > 16)
      decoder_table_bits = static_cast<uint8>(math::minimum(1 + math::ceil_log2i(m_total_syms), prefix_coding::cMaxTableBits));
   return decoder_table_bits;
#endif
}

symbol_codec::symbol_codec() :
  m_pDecode_buf(NULL),
  m_pDecode_buf_next(NULL),
  m_pDecode_buf_end(NULL),
  m_decode_buf_size(0),
  m_bit_buf(0),
  m_bit_count(0)
{
}

// Code length encoding symbols:
// 0-16 - actual code lengths
const uint32 cMaxCodelengthCodes      = 21;

const uint32 cSmallZeroRunCode        = 17;
const uint32 cLargeZeroRunCode        = 18;
const uint32 cSmallRepeatCode         = 19;
const uint32 cLargeRepeatCode         = 20;

const uint32 cMinSmallZeroRunSize     = 3;
const uint32 cMaxSmallZeroRunSize     = 10;
const uint32 cMinLargeZeroRunSize     = 11;
const uint32 cMaxLargeZeroRunSize     = 138;

const uint32 cSmallMinNonZeroRunSize  = 3;
const uint32 cSmallMaxNonZeroRunSize  = 6;
const uint32 cLargeMinNonZeroRunSize  = 7;
const uint32 cLargeMaxNonZeroRunSize  = 70;

const uint32 cSmallZeroRunExtraBits   = 3;
const uint32 cLargeZeroRunExtraBits   = 7;
const uint32 cSmallNonZeroRunExtraBits = 2;
const uint32 cLargeNonZeroRunExtraBits = 6;

static const uint8 g_most_probable_codelength_codes[] =
{
   cSmallZeroRunCode, cLargeZeroRunCode,
   cSmallRepeatCode,  cLargeRepeatCode,

   0, 8,
   7, 9,
   6, 10,
   5, 11,
   4, 12,
   3, 13,
   2, 14,
   1, 15,
   16
};
const uint32 cNumMostProbableCodelengthCodes = sizeof(g_most_probable_codelength_codes) / sizeof(g_most_probable_codelength_codes[0]);

bool symbol_codec::decode_receive_static_data_model(static_huffman_data_model& model)
{
   const uint32 total_used_syms = decode_bits(math::total_bits(prefix_coding::cMaxSupportedSyms));

   if (!total_used_syms)
   {
      model.clear();
      return true;
   }

   if (!model.m_code_sizes.resize(total_used_syms))
      return false;

   memset(&model.m_code_sizes[0], 0, sizeof(model.m_code_sizes[0]) * total_used_syms);

   const uint32 num_codelength_codes_to_send = decode_bits(5);
   if ((num_codelength_codes_to_send < 1) || (num_codelength_codes_to_send > cMaxCodelengthCodes))
      return false;

   static_huffman_data_model dm;
   if (!dm.m_code_sizes.resize(cMaxCodelengthCodes))
      return false;

   for (uint32 i = 0; i < num_codelength_codes_to_send; i++)
      dm.m_code_sizes[g_most_probable_codelength_codes[i]] = static_cast<uint8>(decode_bits(3));

   if (!dm.prepare_decoder_tables())
      return false;

   uint32 ofs = 0;
   while (ofs < total_used_syms)
   {
      const uint32 num_remaining = total_used_syms - ofs;

      uint32 code = decode(dm);
      if (code <= 16)
         model.m_code_sizes[ofs++] = static_cast<uint8>(code);
      else if (code == cSmallZeroRunCode)
      {
         uint32 len = decode_bits(cSmallZeroRunExtraBits) + cMinSmallZeroRunSize;
         if (len > num_remaining)
            return false;
         ofs += len;
      }
      else if (code == cLargeZeroRunCode)
      {
         uint32 len = decode_bits(cLargeZeroRunExtraBits) + cMinLargeZeroRunSize;
         if (len > num_remaining)
            return false;
         ofs += len;
      }
      else if ((code == cSmallRepeatCode) || (code == cLargeRepeatCode))
      {
         uint32 len;
         if (code == cSmallRepeatCode)
            len = decode_bits(cSmallNonZeroRunExtraBits) + cSmallMinNonZeroRunSize;
         else
            len = decode_bits(cLargeNonZeroRunExtraBits) + cLargeMinNonZeroRunSize;

         if ((!ofs) || (len > num_remaining))
            return false;
         const uint32 prev = model.m_code_sizes[ofs - 1];
         if (!prev)
            return false;
         const uint32 end = ofs + len;
         while (ofs < end)
            model.m_code_sizes[ofs++] = static_cast<uint8>(prev);
      }
      else
      {
         CRND_ASSERT(0);
         return false;
      }
   }

   if (ofs != total_used_syms)
      return false;

   return model.prepare_decoder_tables();
}

bool symbol_codec::start_decoding(const uint8* pBuf, uint32 buf_size)
{
   if (!buf_size)
      return false;

   m_pDecode_buf = pBuf;
   m_pDecode_buf_next = pBuf;
   m_decode_buf_size = buf_size;
   m_pDecode_buf_end = pBuf + buf_size;

   get_bits_init();

   return true;
}

void symbol_codec::get_bits_init()
{
   m_bit_buf = 0;
   m_bit_count = 0;
}

uint32 symbol_codec::decode_bits(uint32 num_bits)
{
   if (!num_bits)
      return 0;

   if (num_bits > 16)
   {
      uint32 a = get_bits(num_bits - 16);
      uint32 b = get_bits(16);

      return (a << 16) | b;
   }
   else
      return get_bits(num_bits);
}

uint32 symbol_codec::get_bits(uint32 num_bits)
{
   CRND_ASSERT(num_bits <= 32U);

   while (m_bit_count < (int)num_bits)
   {
      bit_buf_type c = 0;
      if (m_pDecode_buf_next != m_pDecode_buf_end)
         c = *m_pDecode_buf_next++;

      m_bit_count += 8;
      CRND_ASSERT(m_bit_count <= cBitBufSize);

      m_bit_buf |= (c << (cBitBufSize - m_bit_count));
   }

   uint32 result = static_cast<uint32>(m_bit_buf >> (cBitBufSize - num_bits));

   m_bit_buf <<= num_bits;
   m_bit_count -= num_bits;

   return result;
}

uint32 symbol_codec::decode(const static_huffman_data_model& model)
{
   const prefix_coding::decoder_tables* pTables = model.m_pDecode_tables;

   if (m_bit_count < 24)
   {
      if (m_bit_count < 16)
      {
         uint32 c0 = 0, c1 = 0;
         const uint8* p = m_pDecode_buf_next;
         if (p < m_pDecode_buf_end) c0 = *p++;
         if (p < m_pDecode_buf_end) c1 = *p++;
         m_pDecode_buf_next = p;
         m_bit_count += 16;
         uint32 c = (c0 << 8) | c1;
         m_bit_buf |= (c << (32 - m_bit_count));
      }
      else
      {
         uint32 c = (m_pDecode_buf_next < m_pDecode_buf_end) ? *m_pDecode_buf_next++ : 0;
         m_bit_count += 8;
         m_bit_buf |= (c << (32 - m_bit_count));
      }
   }

   uint32 k = (m_bit_buf >> 16) + 1;
   uint32 sym, len;

   if (k <= pTables->m_table_max_code)
   {
      uint32 t = pTables->m_lookup[m_bit_buf >> (32 - pTables->m_table_bits)];

      CRND_ASSERT(t != cUINT32_MAX);
      sym = t & cUINT16_MAX;
      len = t >> 16;

      CRND_ASSERT(model.m_code_sizes[sym] == len);
   }
   else
   {
      len = pTables->m_decode_start_code_size;

      for ( ; ; )
      {
         if (k <= pTables->m_max_codes[len - 1])
            break;
         len++;
      }

      int val_ptr = pTables->m_val_ptrs[len - 1] + (m_bit_buf >> (32 - len));

      if (((uint32)val_ptr >= model.m_total_syms))
      {
         // corrupted stream, or a bug
         CRND_ASSERT(0);
         return 0;
      }

      sym = pTables->m_sorted_symbol_order[val_ptr];
   }

   m_bit_buf <<= len;
   m_bit_count -= len;

   return sym;
}

   uint64 symbol_codec::stop_decoding()
   {
#if 0
      uint32 i = get_bits(4);
      uint32 k = get_bits(3);
      i, k;
      CRND_ASSERT((i == 15) && (k == 3));
#endif

      uint64 n = static_cast<uint64>(m_pDecode_buf_next - m_pDecode_buf);

      return n;
   }

} // namespace crnd

// File: crnd_dxt_hc_common.cpp
namespace crnd
{
   chunk_encoding_desc g_chunk_encodings[cNumChunkEncodings] =
   {
      { 1, { { 0, 0, 8, 8, 0 } } },

      { 2, { { 0, 0, 8, 4, 1 }, { 0, 4, 8, 4, 2 } } },
      { 2, { { 0, 0, 4, 8, 3 }, { 4, 0, 4, 8, 4 } } },

      { 3, { { 0, 0, 8, 4, 1 }, { 0, 4, 4, 4, 7 }, { 4, 4, 4, 4, 8 } } },
      { 3, { { 0, 4, 8, 4, 2 }, { 0, 0, 4, 4, 5 }, { 4, 0, 4, 4, 6 } } },

      { 3, { { 0, 0, 4, 8, 3 }, { 4, 0, 4, 4, 6 }, { 4, 4, 4, 4, 8 } } },
      { 3, { { 4, 0, 4, 8, 4 }, { 0, 0, 4, 4, 5 }, { 0, 4, 4, 4, 7 } } },

      { 4, { { 0, 0, 4, 4, 5 }, { 4, 0, 4, 4, 6 }, { 0, 4, 4, 4, 7 }, { 4, 4, 4, 4, 8 } } }
   };

   chunk_tile_desc g_chunk_tile_layouts[cNumChunkTileLayouts] =
   {
      // 2x2
      { 0, 0, 8, 8, 0 },

      // 2x1
      { 0, 0, 8, 4, 1 },
      { 0, 4, 8, 4, 2 },

      // 1x2
      { 0, 0, 4, 8, 3 },
      { 4, 0, 4, 8, 4 },

      // 1x1
      { 0, 0, 4, 4, 5 },
      { 4, 0, 4, 4, 6 },
      { 0, 4, 4, 4, 7 },
      { 4, 4, 4, 4, 8 }
   };

} // namespace crnd

// File: crnd_dxt.cpp
namespace crnd
{
   const uint8 g_dxt1_to_linear[cDXT1SelectorValues]     = { 0U, 3U, 1U, 2U };
   const uint8 g_dxt1_from_linear[cDXT1SelectorValues]   = { 0U, 2U, 3U, 1U };

   const uint8 g_dxt5_to_linear[cDXT5SelectorValues]     = { 0U, 7U, 1U, 2U, 3U, 4U, 5U, 6U };
   const uint8 g_dxt5_from_linear[cDXT5SelectorValues]   = { 0U, 2U, 3U, 4U, 5U, 6U, 7U, 1U };

   const uint8 g_six_alpha_invert_table[cDXT5SelectorValues] = { 1, 0, 5, 4, 3, 2, 6, 7 };
   const uint8 g_eight_alpha_invert_table[cDXT5SelectorValues] = { 1, 0, 7, 6, 5, 4, 3, 2 };

   uint16 dxt1_block::pack_color(const color_quad_u8& color, bool scaled, uint32 bias)
   {
      uint32 r = color.r;
      uint32 g = color.g;
      uint32 b = color.b;

      if (scaled)
      {
         r = (r * 31U + bias) / 255U;
         g = (g * 63U + bias) / 255U;
         b = (b * 31U + bias) / 255U;
      }

      r = math::minimum(r, 31U);
      g = math::minimum(g, 63U);
      b = math::minimum(b, 31U);

      return static_cast<uint16>(b | (g << 5U) | (r << 11U));
   }

   uint16 dxt1_block::pack_color(uint32 r, uint32 g, uint32 b, bool scaled, uint32 bias)
   {
      return pack_color(color_quad_u8(r, g, b, 0), scaled, bias);
   }

   color_quad_u8 dxt1_block::unpack_color(uint16 packed_color, bool scaled, uint32 alpha)
   {
      uint32 b = packed_color & 31U;
      uint32 g = (packed_color >> 5U) & 63U;
      uint32 r = (packed_color >> 11U) & 31U;

      if (scaled)
      {
         b = (b << 3U) | (b >> 2U);
         g = (g << 2U) | (g >> 4U);
         r = (r << 3U) | (r >> 2U);
      }

      return color_quad_u8(r, g, b, alpha);
   }

   void dxt1_block::unpack_color(uint32& r, uint32& g, uint32& b, uint16 packed_color, bool scaled)
   {
      color_quad_u8 c(unpack_color(packed_color, scaled, 0));
      r = c.r;
      g = c.g;
      b = c.b;
   }

   uint32 dxt1_block::get_block_colors3(color_quad_u8* pDst, uint16 color0, uint16 color1)
   {
      color_quad_u8 c0(unpack_color(color0, true));
      color_quad_u8 c1(unpack_color(color1, true));

      pDst[0] = c0;
      pDst[1] = c1;
      pDst[2].set( (c0.r + c1.r) >> 1U, (c0.g + c1.g) >> 1U, (c0.b + c1.b) >> 1U, 255U);
      pDst[3].set(0, 0, 0, 0);

      return 3;
   }

   uint32 dxt1_block::get_block_colors4(color_quad_u8* pDst, uint16 color0, uint16 color1)
   {
      color_quad_u8 c0(unpack_color(color0, true));
      color_quad_u8 c1(unpack_color(color1, true));

      pDst[0] = c0;
      pDst[1] = c1;

      // 12/14/09 - Supposed to round according to DX docs, but this conflicts with the OpenGL S3TC spec. ?
      // Turns out some GPU's round and some don't. Great.
      //pDst[2].set( (c0.r * 2 + c1.r + 1) / 3, (c0.g * 2 + c1.g + 1) / 3, (c0.b * 2 + c1.b + 1) / 3, 255U);
      //pDst[3].set( (c1.r * 2 + c0.r + 1) / 3, (c1.g * 2 + c0.g + 1) / 3, (c1.b * 2 + c0.b + 1) / 3, 255U);

      pDst[2].set( (c0.r * 2 + c1.r) / 3, (c0.g * 2 + c1.g) / 3, (c0.b * 2 + c1.b) / 3, 255U);
      pDst[3].set( (c1.r * 2 + c0.r) / 3, (c1.g * 2 + c0.g) / 3, (c1.b * 2 + c0.b) / 3, 255U);

      return 4;
   }

   uint32 dxt1_block::get_block_colors(color_quad_u8* pDst, uint16 color0, uint16 color1)
   {
      if (color0 > color1)
         return get_block_colors4(pDst, color0, color1);
      else
         return get_block_colors3(pDst, color0, color1);
   }

   color_quad_u8 dxt1_block::unpack_endpoint(uint32 endpoints, uint32 index, bool scaled, uint32 alpha)
   {
      CRND_ASSERT(index < 2);
      return unpack_color( static_cast<uint16>((endpoints >> (index * 16U)) & 0xFFFFU), scaled, alpha );
   }

   uint32 dxt1_block::pack_endpoints(uint32 lo, uint32 hi)
   {
      CRND_ASSERT((lo <= 0xFFFFU) && (hi <= 0xFFFFU));
      return lo | (hi << 16U);
   }

   void dxt3_block::set_alpha(uint32 x, uint32 y, uint32 value, bool scaled)
   {
      CRND_ASSERT((x < cDXTBlockSize) && (y < cDXTBlockSize));

      if (scaled)
      {
         CRND_ASSERT(value <= 0xFF);
         value = (value * 15U + 128U) / 255U;
      }
      else
      {
         CRND_ASSERT(value <= 0xF);
      }

      uint32 ofs = (y << 1U) + (x >> 1U);
      uint32 c = m_alpha[ofs];

      c &= ~(0xF << ((x & 1U) << 2U));
      c |= (value << ((x & 1U) << 2U));

      m_alpha[ofs] = static_cast<uint8>(c);
   }

   uint32 dxt3_block::get_alpha(uint32 x, uint32 y, bool scaled) const
   {
      CRND_ASSERT((x < cDXTBlockSize) && (y < cDXTBlockSize));

      uint32 value = m_alpha[(y << 1U) + (x >> 1U)];
      if (x & 1)
         value >>= 4;
      value &= 0xF;

      if (scaled)
         value = (value << 4U) | value;

      return value;
   }

   uint32 dxt5_block::get_block_values6(color_quad_u8* pDst, uint32 l, uint32 h)
   {
      pDst[0].a = static_cast<uint8>(l);
      pDst[1].a = static_cast<uint8>(h);
      pDst[2].a = static_cast<uint8>((l * 4 + h    ) / 5);
      pDst[3].a = static_cast<uint8>((l * 3 + h * 2) / 5);
      pDst[4].a = static_cast<uint8>((l * 2 + h * 3) / 5);
      pDst[5].a = static_cast<uint8>((l     + h * 4) / 5);
      pDst[6].a = 0;
      pDst[7].a = 255;
      return 6;
   }

   uint32 dxt5_block::get_block_values8(color_quad_u8* pDst, uint32 l, uint32 h)
   {
      pDst[0].a = static_cast<uint8>(l);
      pDst[1].a = static_cast<uint8>(h);
      pDst[2].a = static_cast<uint8>((l * 6 + h    ) / 7);
      pDst[3].a = static_cast<uint8>((l * 5 + h * 2) / 7);
      pDst[4].a = static_cast<uint8>((l * 4 + h * 3) / 7);
      pDst[5].a = static_cast<uint8>((l * 3 + h * 4) / 7);
      pDst[6].a = static_cast<uint8>((l * 2 + h * 5) / 7);
      pDst[7].a = static_cast<uint8>((l     + h * 6) / 7);
      return 8;
   }

   uint32 dxt5_block::get_block_values(color_quad_u8* pDst, uint32 l, uint32 h)
   {
      if (l > h)
         return get_block_values8(pDst, l, h);
      else
         return get_block_values6(pDst, l, h);
   }

   uint32 dxt5_block::get_block_values6(uint32* pDst, uint32 l, uint32 h)
   {
      pDst[0] = l;
      pDst[1] = h;
      pDst[2] = (l * 4 + h    ) / 5;
      pDst[3] = (l * 3 + h * 2) / 5;
      pDst[4] = (l * 2 + h * 3) / 5;
      pDst[5] = (l     + h * 4) / 5;
      pDst[6] = 0;
      pDst[7] = 255;
      return 6;
   }

   uint32 dxt5_block::get_block_values8(uint32* pDst, uint32 l, uint32 h)
   {
      pDst[0] = l;
      pDst[1] = h;
      pDst[2] = (l * 6 + h    ) / 7;
      pDst[3] = (l * 5 + h * 2) / 7;
      pDst[4] = (l * 4 + h * 3) / 7;
      pDst[5] = (l * 3 + h * 4) / 7;
      pDst[6] = (l * 2 + h * 5) / 7;
      pDst[7] = (l     + h * 6) / 7;
      return 8;
   }

   uint32 dxt5_block::unpack_endpoint(uint32 packed, uint32 index)
   {
      CRND_ASSERT(index < 2);
      return (packed >> (8 * index)) & 0xFF;
   }

   uint32 dxt5_block::pack_endpoints(uint32 lo, uint32 hi)
   {
      CRND_ASSERT((lo <= 0xFF) && (hi <= 0xFF));
      return lo | (hi << 8U);
   }

   uint32 dxt5_block::get_block_values(uint32* pDst, uint32 l, uint32 h)
   {
      if (l > h)
         return get_block_values8(pDst, l, h);
      else
         return get_block_values6(pDst, l, h);
   }

} // namespace crnd

// File: crnd_decode.cpp
#define CRND_CREATE_BYTE_STREAMS 0

namespace crnd
{
#if CRND_CREATE_BYTE_STREAMS
   static void write_array_to_file(const char* pFilename, const vector<uint8>& buf)
   {
      FILE* pFile = fopen(pFilename, "wb");
      fwrite(&buf[0], buf.size(), 1, pFile);
      fclose(pFile);
   }
#endif

   struct crnd_chunk_tile_desc
   {
      // These values are in blocks
      uint8 m_x_ofs;
      uint8 m_y_ofs;
      uint8 m_width;
      uint8 m_height;
   };

   struct crnd_chunk_encoding_desc
   {
      uint32 m_num_tiles;
      chunk_tile_desc m_tiles[4];
   };

#if 0
   static crnd_chunk_encoding_desc g_crnd_chunk_encodings[cNumChunkEncodings] =
   {
      { 1, { { 0, 0, 2, 2 } } },

      { 2, { { 0, 0, 2, 1 }, { 0, 1, 2, 1 } } },
      { 2, { { 0, 0, 1, 2 }, { 1, 0, 1, 2 } } },

      { 3, { { 0, 0, 2, 1 }, { 0, 1, 1, 1 }, { 1, 1, 1, 1 } } },
      { 3, { { 0, 1, 2, 1 }, { 0, 0, 1, 1 }, { 1, 0, 1, 1 } } },

      { 3, { { 0, 0, 1, 2 }, { 1, 0, 1, 1 }, { 1, 1, 1, 1 } } },
      { 3, { { 1, 0, 1, 2 }, { 0, 0, 1, 1 }, { 0, 1, 1, 1 } } },

      { 1, { { 0, 0, 1, 1 }, { 1, 0, 1, 1 }, { 0, 1, 1, 1 }, { 1, 1, 1, 1 } } }
   };
#endif

   struct crnd_encoding_tile_indices
   {
      uint8 m_tiles[4];
   };

   static crnd_encoding_tile_indices g_crnd_chunk_encoding_tiles[cNumChunkEncodings] =
   {
      { { 0, 0, 0, 0 } },

      { { 0, 0, 1, 1 } },
      { { 0, 1, 0, 1 } },

      { { 0, 0, 1, 2 } },
      { { 1, 2, 0, 0 } },

      { { 0, 1, 0, 2 } },
      { { 1, 0, 2, 0 } },

      { { 0, 1, 2, 3 } }
   };

   static uint8 g_crnd_chunk_encoding_num_tiles[cNumChunkEncodings] = { 1, 2, 2, 3, 3, 3, 3, 4 };

   class crn_unpacker
   {
   public:
      inline crn_unpacker() :
         m_magic(cMagicValue),
         m_pData(NULL),
         m_data_size(0),
         m_pHeader(NULL)
      {
      }

      inline ~crn_unpacker()
      {
         m_magic = 0;
      }

      inline bool is_valid() const { return m_magic == cMagicValue; }

      bool init(const void* pData, uint32 data_size)
      {
         m_pHeader = crnd_get_header(m_tmp_header, pData, data_size);
         if (!m_pHeader)
            return false;

         m_pData = static_cast<const uint8*>(pData);
         m_data_size = data_size;

         if (!init_tables())
            return false;

         if (!decode_palettes())
            return false;

         return true;
      }

      bool unpack_level(
         void** pDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes,
         uint32 level_index)
      {
         uint32 cur_level_ofs = m_pHeader->m_level_ofs[level_index];

         uint32 next_level_ofs = m_data_size;
         if ((level_index + 1) < (m_pHeader->m_levels))
            next_level_ofs = m_pHeader->m_level_ofs[level_index + 1];

         CRND_ASSERT(next_level_ofs > cur_level_ofs);

         return unpack_level(m_pData + cur_level_ofs, next_level_ofs - cur_level_ofs, pDst, dst_size_in_bytes, row_pitch_in_bytes, level_index);
      }

      bool unpack_level(
         const void* pSrc, uint32 src_size_in_bytes,
         void** pDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes,
         uint32 level_index)
      {
         dst_size_in_bytes;

#ifdef CRND_BUILD_DEBUG
         for (uint32 f = 0; f < m_pHeader->m_faces; f++)
            if (!pDst[f])
               return false;
#endif

         const uint32 width = math::maximum(m_pHeader->m_width >> level_index, 1U);
         const uint32 height = math::maximum(m_pHeader->m_height >> level_index, 1U);
         const uint32 blocks_x = (width + 3U) >> 2U;
         const uint32 blocks_y = (height + 3U) >> 2U;
         const uint32 block_size = ((m_pHeader->m_format == cCRNFmtDXT1) || (m_pHeader->m_format == cCRNFmtDXT5A)) ? 8 : 16;

         uint32 minimal_row_pitch = block_size * blocks_x;
         if (!row_pitch_in_bytes)
            row_pitch_in_bytes = minimal_row_pitch;
         else if ((row_pitch_in_bytes < minimal_row_pitch) || (row_pitch_in_bytes & 3))
            return false;
         if (dst_size_in_bytes < row_pitch_in_bytes * blocks_y)
            return false;

         const uint32 chunks_x = (blocks_x + 1) >> 1;
         const uint32 chunks_y = (blocks_y + 1) >> 1;

#if CRND_CREATE_BYTE_STREAMS
         crnd_trace("Index stream: %u bytes\n", src_size_in_bytes);
#endif

         if (!m_codec.start_decoding(static_cast<const crnd::uint8*>(pSrc), src_size_in_bytes))
            return false;

         bool status = false;
         switch (m_pHeader->m_format)
         {
         case cCRNFmtDXT1:
            status = unpack_dxt1((uint8**)pDst, dst_size_in_bytes, row_pitch_in_bytes, blocks_x, blocks_y, chunks_x, chunks_y);
            break;
         case cCRNFmtDXT5:
         case cCRNFmtDXT5_CCxY:
         case cCRNFmtDXT5_xGBR:
         case cCRNFmtDXT5_AGBR:
         case cCRNFmtDXT5_xGxR:
            status = unpack_dxt5((uint8**)pDst, dst_size_in_bytes, row_pitch_in_bytes, blocks_x, blocks_y, chunks_x, chunks_y);
            break;
         case cCRNFmtDXT5A:
            status = unpack_dxt5a((uint8**)pDst, dst_size_in_bytes, row_pitch_in_bytes, blocks_x, blocks_y, chunks_x, chunks_y);
            break;
         case cCRNFmtDXN_XY:
         case cCRNFmtDXN_YX:
            status = unpack_dxn((uint8**)pDst, dst_size_in_bytes, row_pitch_in_bytes, blocks_x, blocks_y, chunks_x, chunks_y);
            break;
         default:
            return false;
         }
         if (!status)
            return false;

         m_codec.stop_decoding();
         return true;
      }

      inline const void* get_data() const { return m_pData; }
      inline uint32 get_data_size() const { return m_data_size; }

   private:
      enum { cMagicValue = 0x1EF9CABD };
      uint32             m_magic;

      const uint8*       m_pData;
      uint32             m_data_size;
      crn_header         m_tmp_header;
      const crn_header*  m_pHeader;

      symbol_codec       m_codec;

      static_huffman_data_model m_chunk_encoding_dm;
      static_huffman_data_model m_endpoint_delta_dm[2];
      static_huffman_data_model m_selector_delta_dm[2];

      crnd::vector<uint32> m_color_endpoints;
      crnd::vector<uint32> m_color_selectors;

      crnd::vector<uint16> m_alpha_endpoints;
      crnd::vector<uint16> m_alpha_selectors;

      bool init_tables()
      {
         if (!m_codec.start_decoding(m_pData + m_pHeader->m_tables_ofs, m_pHeader->m_tables_size))
            return false;

         if (!m_codec.decode_receive_static_data_model(m_chunk_encoding_dm))
            return false;

         if ((!m_pHeader->m_color_endpoints.m_num) && (!m_pHeader->m_alpha_endpoints.m_num))
            return false;

         if (m_pHeader->m_color_endpoints.m_num)
         {
            if (!m_codec.decode_receive_static_data_model(m_endpoint_delta_dm[0])) return false;
            if (!m_codec.decode_receive_static_data_model(m_selector_delta_dm[0])) return false;
         }

         if (m_pHeader->m_alpha_endpoints.m_num)
         {
            if (!m_codec.decode_receive_static_data_model(m_endpoint_delta_dm[1])) return false;
            if (!m_codec.decode_receive_static_data_model(m_selector_delta_dm[1])) return false;
         }

         m_codec.stop_decoding();

         return true;
      }

      bool decode_palettes()
      {
         if (m_pHeader->m_color_endpoints.m_num)
         {
            if (!decode_color_endpoints()) return false;
            if (!decode_color_selectors()) return false;
         }

         if (m_pHeader->m_alpha_endpoints.m_num)
         {
            if (!decode_alpha_endpoints()) return false;
            if (!decode_alpha_selectors()) return false;
         }

         return true;
      }

      bool decode_color_endpoints()
      {
         const uint32 num_color_endpoints = m_pHeader->m_color_endpoints.m_num;

         if (!m_color_endpoints.resize(num_color_endpoints))
            return false;

         if (!m_codec.start_decoding(m_pData + m_pHeader->m_color_endpoints.m_ofs, m_pHeader->m_color_endpoints.m_size))
            return false;

         static_huffman_data_model dm[2];
         for (uint32 i = 0; i < 2; i++)
            if (!m_codec.decode_receive_static_data_model(dm[i]))
               return false;

         uint32 a = 0, b = 0, c = 0;
         uint32 d = 0, e = 0, f = 0;

         uint32* CRND_RESTRICT pDst = &m_color_endpoints[0];

         CRND_HUFF_DECODE_BEGIN(m_codec);

#if CRND_CREATE_BYTE_STREAMS
         vector<uint8> byte_stream;
#endif

         for (uint32 i = 0; i < num_color_endpoints; i++)
         {
            uint32 da, db, dc, dd, de, df;
            CRND_HUFF_DECODE(m_codec, dm[0], da); a = (a + da) & 31;
            CRND_HUFF_DECODE(m_codec, dm[1], db); b = (b + db) & 63;
            CRND_HUFF_DECODE(m_codec, dm[0], dc); c = (c + dc) & 31;

            CRND_HUFF_DECODE(m_codec, dm[0], dd); d = (d + dd) & 31;
            CRND_HUFF_DECODE(m_codec, dm[1], de); e = (e + de) & 63;
            CRND_HUFF_DECODE(m_codec, dm[0], df); f = (f + df) & 31;

#if CRND_CREATE_BYTE_STREAMS
            byte_stream.push_back(da);
            byte_stream.push_back(db);
            byte_stream.push_back(dc);
            byte_stream.push_back(dd);
            byte_stream.push_back(de);
            byte_stream.push_back(df);
#endif

            if (c_crnd_little_endian_platform)
               *pDst++ = c | (b << 5U) | (a << 11U) | (f << 16U) | (e << 21U) | (d << 27U);
            else
               *pDst++ = f | (e << 5U) | (d << 11U) | (c << 16U) | (b << 21U) | (a << 27U);
         }

         CRND_HUFF_DECODE_END(m_codec);

         m_codec.stop_decoding();

#if CRND_CREATE_BYTE_STREAMS
         write_array_to_file(L"colorendpoints.bin", byte_stream);
         crnd_trace("color endpoints: %u\n", (uint)m_pHeader->m_color_endpoints.m_size);
#endif

         return true;
      }

      bool decode_color_selectors()
      {
         const uint32 cMaxSelectorValue = 3U;
         const uint32 cMaxUniqueSelectorDeltas = cMaxSelectorValue * 2U + 1U;

         const uint32 num_color_selectors = m_pHeader->m_color_selectors.m_num;

         if (!m_codec.start_decoding(m_pData + m_pHeader->m_color_selectors.m_ofs, m_pHeader->m_color_selectors.m_size))
            return false;

         static_huffman_data_model dm;
         if (!m_codec.decode_receive_static_data_model(dm))
            return false;

         int32 delta0[cMaxUniqueSelectorDeltas * cMaxUniqueSelectorDeltas];
         int32 delta1[cMaxUniqueSelectorDeltas * cMaxUniqueSelectorDeltas];
         int32 l = -(int32)cMaxSelectorValue, m = -(int32)cMaxSelectorValue;
         for (uint32 i = 0; i < (cMaxUniqueSelectorDeltas * cMaxUniqueSelectorDeltas); i++)
         {
            delta0[i] = l;
            delta1[i] = m;

            if (++l > (int32)cMaxSelectorValue)
            {
               l = -(int32)cMaxSelectorValue;
               m++;
            }
         }

         uint32 cur[16];
         utils::zero_object(cur);

         if (!m_color_selectors.resize(num_color_selectors))
            return false;

         uint32* CRND_RESTRICT pDst = &m_color_selectors[0];

         const uint8* pFrom_linear = g_dxt1_from_linear;

         CRND_HUFF_DECODE_BEGIN(m_codec);

#if CRND_CREATE_BYTE_STREAMS
         vector<uint8> byte_stream;
#endif

         for (uint32 i = 0; i < num_color_selectors; i++)
         {
            for (uint32 j = 0; j < 8; j++)
            {
               int32 sym;
               CRND_HUFF_DECODE(m_codec, dm, sym);

#if CRND_CREATE_BYTE_STREAMS
               byte_stream.push_back(sym);
#endif

               cur[j*2+0] = (delta0[sym] + cur[j*2+0]) & 3;
               cur[j*2+1] = (delta1[sym] + cur[j*2+1]) & 3;
            }

            if (c_crnd_little_endian_platform)
            {
               *pDst++ =
                  (pFrom_linear[cur[0 ]]      ) | (pFrom_linear[cur[1 ]] <<  2) | (pFrom_linear[cur[2 ]] <<  4) | (pFrom_linear[cur[3 ]] <<  6) |
                  (pFrom_linear[cur[4 ]] <<  8) | (pFrom_linear[cur[5 ]] << 10) | (pFrom_linear[cur[6 ]] << 12) | (pFrom_linear[cur[7 ]] << 14) |
                  (pFrom_linear[cur[8 ]] << 16) | (pFrom_linear[cur[9 ]] << 18) | (pFrom_linear[cur[10]] << 20) | (pFrom_linear[cur[11]] << 22) |
                  (pFrom_linear[cur[12]] << 24) | (pFrom_linear[cur[13]] << 26) | (pFrom_linear[cur[14]] << 28) | (pFrom_linear[cur[15]] << 30);
            }
            else
            {
               *pDst++ =
                  (pFrom_linear[cur[8 ]]      ) | (pFrom_linear[cur[9 ]] <<  2) | (pFrom_linear[cur[10]] <<  4) | (pFrom_linear[cur[11]] <<  6) |
                  (pFrom_linear[cur[12]] <<  8) | (pFrom_linear[cur[13]] << 10) | (pFrom_linear[cur[14]] << 12) | (pFrom_linear[cur[15]] << 14) |
                  (pFrom_linear[cur[0 ]] << 16) | (pFrom_linear[cur[1 ]] << 18) | (pFrom_linear[cur[2 ]] << 20) | (pFrom_linear[cur[3 ]] << 22) |
                  (pFrom_linear[cur[4 ]] << 24) | (pFrom_linear[cur[5 ]] << 26) | (pFrom_linear[cur[6 ]] << 28) | (pFrom_linear[cur[7 ]] << 30);
            }
         }

         CRND_HUFF_DECODE_END(m_codec);

         m_codec.stop_decoding();

#if CRND_CREATE_BYTE_STREAMS
         write_array_to_file(L"colorselectors.bin", byte_stream);
         crnd_trace("color selectors: %u\n", (uint)m_pHeader->m_color_selectors.m_size);
#endif

         return true;
      }

      bool decode_alpha_endpoints()
      {
         const uint32 num_alpha_endpoints = m_pHeader->m_alpha_endpoints.m_num;

         if (!m_codec.start_decoding(m_pData + m_pHeader->m_alpha_endpoints.m_ofs, m_pHeader->m_alpha_endpoints.m_size))
            return false;

         static_huffman_data_model dm;
         if (!m_codec.decode_receive_static_data_model(dm))
            return false;

         if (!m_alpha_endpoints.resize(num_alpha_endpoints))
            return false;

         uint16* CRND_RESTRICT pDst = &m_alpha_endpoints[0];
         uint32 a = 0, b = 0;

         CRND_HUFF_DECODE_BEGIN(m_codec);

         for (uint32 i = 0; i < num_alpha_endpoints; i++)
         {
            uint sa; CRND_HUFF_DECODE(m_codec, dm, sa);
            uint sb; CRND_HUFF_DECODE(m_codec, dm, sb);

            a = (sa + a) & 255;
            b = (sb + b) & 255;

            *pDst++ = (uint16)(a | (b << 8));
         }

         CRND_HUFF_DECODE_END(m_codec);

         m_codec.stop_decoding();

         return true;
      }

      bool decode_alpha_selectors()
      {
         const uint32 cMaxSelectorValue = 7U;
         const uint32 cMaxUniqueSelectorDeltas = cMaxSelectorValue * 2U + 1U;

         const uint32 num_alpha_selectors = m_pHeader->m_alpha_selectors.m_num;

         if (!m_codec.start_decoding(m_pData + m_pHeader->m_alpha_selectors.m_ofs, m_pHeader->m_alpha_selectors.m_size))
            return false;

         static_huffman_data_model dm;
         if (!m_codec.decode_receive_static_data_model(dm))
            return false;

         int32 delta0[cMaxUniqueSelectorDeltas * cMaxUniqueSelectorDeltas];
         int32 delta1[cMaxUniqueSelectorDeltas * cMaxUniqueSelectorDeltas];
         int32 l = -(int32)cMaxSelectorValue, m = -(int32)cMaxSelectorValue;
         for (uint32 i = 0; i < (cMaxUniqueSelectorDeltas * cMaxUniqueSelectorDeltas); i++)
         {
            delta0[i] = l;
            delta1[i] = m;

            if (++l > (int32)cMaxSelectorValue)
            {
               l = -(int32)cMaxSelectorValue;
               m++;
            }
         }

         uint32 cur[16];
         utils::zero_object(cur);

         if (!m_alpha_selectors.resize(num_alpha_selectors * 3))
            return false;

         uint16* CRND_RESTRICT pDst = &m_alpha_selectors[0];

         const uint8* pFrom_linear = g_dxt5_from_linear;

         CRND_HUFF_DECODE_BEGIN(m_codec);

         for (uint32 i = 0; i < num_alpha_selectors; i++)
         {
            for (uint32 j = 0; j < 8; j++)
            {
               int32 sym;
               CRND_HUFF_DECODE(m_codec, dm, sym);

               cur[j*2+0] = (delta0[sym] + cur[j*2+0]) & 7;
               cur[j*2+1] = (delta1[sym] + cur[j*2+1]) & 7;
               //cur[j*2+0] = ((sym%15)-7 + cur[j*2+0]) & 7;
               //cur[j*2+1] = ((sym/15)-7 + cur[j*2+1]) & 7;
            }

#if 0
            dxt5_block blk;
            for (uint32 y = 0; y < 4; y++)
               for (uint32 x = 0; x < 4; x++)
                  blk.set_selector(x, y, pFrom_linear[cur[x+y*4]]);

            *pDst++ = blk.get_selectors_as_word(0);
            *pDst++ = blk.get_selectors_as_word(1);
            *pDst++ = blk.get_selectors_as_word(2);
#else
            *pDst++ = (uint16)((pFrom_linear[cur[0 ]]      ) | (pFrom_linear[cur[1 ]] <<  3) | (pFrom_linear[cur[2 ]] <<  6) | (pFrom_linear[cur[3 ]] <<  9) |
               (pFrom_linear[cur[4 ]] << 12) | (pFrom_linear[cur[5 ]] << 15));

            *pDst++ = (uint16)((pFrom_linear[cur[5 ]] >> 1) | (pFrom_linear[cur[6 ]] << 2) | (pFrom_linear[cur[7 ]] << 5) |
               (pFrom_linear[cur[8 ]] << 8) | (pFrom_linear[cur[9 ]] << 11) | (pFrom_linear[cur[10]] << 14));

            *pDst++ = (uint16)((pFrom_linear[cur[10]] >> 2) | (pFrom_linear[cur[11]] << 1) | (pFrom_linear[cur[12]] << 4) |
               (pFrom_linear[cur[13]] << 7) | (pFrom_linear[cur[14]] << 10) | (pFrom_linear[cur[15]] << 13));
#endif
         }

         CRND_HUFF_DECODE_END(m_codec);

         m_codec.stop_decoding();

         return true;
      }

      static inline uint32 tiled_offset_2d_outer(uint32 y, uint32 AlignedWidth, uint32 LogBpp)
      {
         uint32 Macro        = ((y >> 5) * (AlignedWidth >> 5)) << (LogBpp + 7);
         uint32 Micro        = ((y & 6) << 2) << LogBpp;

         return Macro +
            ((Micro & ~15) << 1) +
            (Micro & 15) +
            ((y & 8) << (3 + LogBpp)) + ((y & 1) << 4);
      }

      static inline uint32 tiled_offset_2d_inner(uint32 x, uint32 y, uint32 LogBpp, uint32 BaseOffset)
      {
         uint32 Macro = (x >> 5) << (LogBpp + 7);
         uint32 Micro = (x & 7) << LogBpp;
         uint32 Offset  = BaseOffset + Macro + ((Micro & ~15) << 1) + (Micro & 15);

         return ((Offset & ~511) << 3) + ((Offset & 448) << 2) + (Offset & 63) +
            ((y & 16) << 7) +
            (((((y & 8) >> 2) + (x >> 3)) & 3) << 6);
      }

      static inline void limit(uint& x, uint n)
      {
         int v = x - n;
         int msk = (v >> 31);
         x = (x & msk) | (v & ~msk);
      }

      bool unpack_dxt1(uint8** pDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes, uint32 blocks_x, uint32 blocks_y, uint32 chunks_x, uint32 chunks_y)
      {
         dst_size_in_bytes;

         uint32 chunk_encoding_bits = 1;

         const uint32 num_color_endpoints = m_color_endpoints.size();
         const uint32 num_color_selectors = m_color_selectors.size();

         uint32 prev_color_endpoint_index = 0;
         uint32 prev_color_selector_index = 0;

         const uint32 num_faces = m_pHeader->m_faces;

         const uint32 row_pitch_in_dwords = row_pitch_in_bytes >> 2U;

         const int32 cBytesPerBlock = 8;

         CRND_HUFF_DECODE_BEGIN(m_codec);

#if CRND_CREATE_BYTE_STREAMS
         vector<uint8> tile_encoding_stream;
         vector<uint8> endpoint_indices_stream;
         vector<uint8> selector_indices_stream;
#endif

         for (uint32 f = 0; f < num_faces; f++)
         {
            uint8* CRND_RESTRICT pRow = pDst[f];

            for (uint32 y = 0; y < chunks_y; y++)
            {
               int32 start_x = 0;
               int32 end_x = chunks_x;
               int32 dir_x = 1;
               int32 block_delta = cBytesPerBlock*2;
               uint8* CRND_RESTRICT pBlock = pRow;

               if (y & 1)
               {
                  start_x = chunks_x - 1;
                  end_x = -1;
                  dir_x = -1;
                  block_delta = -cBytesPerBlock*2;
                  pBlock += (chunks_x - 1) * cBytesPerBlock * 2;
               }

               const bool skip_bottom_row = (y == (chunks_y - 1)) && (blocks_y & 1);

               for (int32 x = start_x; x != end_x; x += dir_x)
               {
                  uint32 color_endpoints[4];

                  if (chunk_encoding_bits == 1)
                  {
                     CRND_HUFF_DECODE(m_codec, m_chunk_encoding_dm, chunk_encoding_bits);
#if CRND_CREATE_BYTE_STREAMS
                     tile_encoding_stream.push_back(chunk_encoding_bits & 7);
                     tile_encoding_stream.push_back((chunk_encoding_bits >> 3) & 7);
                     tile_encoding_stream.push_back((chunk_encoding_bits >> 6) & 7);
#endif
                     chunk_encoding_bits |= 512;
                  }

                  const uint32 chunk_encoding_index = chunk_encoding_bits & 7;
                  chunk_encoding_bits >>= 3;

                  const uint32 num_tiles = g_crnd_chunk_encoding_num_tiles[chunk_encoding_index];

                  for (uint32 i = 0; i < num_tiles; i++)
                  {
                     uint32 delta;
                     CRND_HUFF_DECODE(m_codec, m_endpoint_delta_dm[0], delta);
#if CRND_CREATE_BYTE_STREAMS
                     endpoint_indices_stream.push_back(delta);
#endif
                     prev_color_endpoint_index += delta;
                     limit(prev_color_endpoint_index, num_color_endpoints);
                     color_endpoints[i] = m_color_endpoints[prev_color_endpoint_index];
                  }

                  const uint8* pTile_indices = g_crnd_chunk_encoding_tiles[chunk_encoding_index].m_tiles;

                  const bool skip_right_col = (blocks_x & 1) && (x == ((int32)chunks_x - 1));

                  uint32* CRND_RESTRICT pD = (uint32*)pBlock;

                  if ((!skip_bottom_row) && (!skip_right_col))
                  {
                     //CRND_ASSERT( ((uint8*)&pD[4 + row_pitch_in_dwords] - pDst) <= dst_size_in_bytes );

                     pD[0] = color_endpoints[pTile_indices[0]];
                     CRND_WRITE_BARRIER
                     uint32 delta0;
                     CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[0], delta0);
#if CRND_CREATE_BYTE_STREAMS
                     selector_indices_stream.push_back(delta0);
#endif
                     prev_color_selector_index += delta0;
                     limit(prev_color_selector_index, num_color_selectors);
                     pD[1] = m_color_selectors[prev_color_selector_index];
                     CRND_WRITE_BARRIER

                     pD[2] = color_endpoints[pTile_indices[1]];
                     CRND_WRITE_BARRIER
                     uint32 delta1;
                     CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[0], delta1);
#if CRND_CREATE_BYTE_STREAMS
                     selector_indices_stream.push_back(delta1);
#endif
                     prev_color_selector_index += delta1;
                     limit(prev_color_selector_index, num_color_selectors);
                     pD[3] = m_color_selectors[prev_color_selector_index];
                     CRND_WRITE_BARRIER

                     pD[0 + row_pitch_in_dwords] = color_endpoints[pTile_indices[2]];
                     CRND_WRITE_BARRIER
                     uint32 delta2;
                     CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[0], delta2);
#if CRND_CREATE_BYTE_STREAMS
                     selector_indices_stream.push_back(delta2);
#endif
                     prev_color_selector_index += delta2;
                     limit(prev_color_selector_index, num_color_selectors);
                     pD[1 + row_pitch_in_dwords] = m_color_selectors[prev_color_selector_index];
                     CRND_WRITE_BARRIER

                     pD[2 + row_pitch_in_dwords] = color_endpoints[pTile_indices[3]];
                     CRND_WRITE_BARRIER
                     uint32 delta3;
                     CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[0], delta3);
#if CRND_CREATE_BYTE_STREAMS
                     selector_indices_stream.push_back(delta3);
#endif
                     prev_color_selector_index += delta3;
                     limit(prev_color_selector_index, num_color_selectors);
                     pD[3 + row_pitch_in_dwords] = m_color_selectors[prev_color_selector_index];
                     CRND_WRITE_BARRIER
                  }
                  else
                  {
                     for (uint32 by = 0; by < 2; by++)
                     {
                        pD = (uint32*)((uint8*)pBlock + row_pitch_in_bytes * by);
                        for (uint32 bx = 0; bx < 2; bx++, pD += 2)
                        {
                           uint32 delta;
                           CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[0], delta);
#if CRND_CREATE_BYTE_STREAMS
                           selector_indices_stream.push_back(delta);
#endif
                           prev_color_selector_index += delta;
                           limit(prev_color_selector_index, num_color_selectors);

                           if (!((bx && skip_right_col) || (by && skip_bottom_row)))
                           {
                              pD[0] = color_endpoints[pTile_indices[bx + by * 2]];
                              CRND_WRITE_BARRIER
                              pD[1] = m_color_selectors[prev_color_selector_index];
                              CRND_WRITE_BARRIER
                           }
                        }
                     }
                  }

                  pBlock += block_delta;

               } // x

               pRow += row_pitch_in_bytes * 2;

            } // y

         } // f

         CRND_HUFF_DECODE_END(m_codec);

#if CRND_CREATE_BYTE_STREAMS
         write_array_to_file(L"tile_encodings.bin", tile_encoding_stream);
         write_array_to_file(L"endpoint_indices.bin", endpoint_indices_stream);
         write_array_to_file(L"selector_indices.bin", selector_indices_stream);
#endif

         return true;
      }

      bool unpack_dxt5(uint8** pDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes, uint32 blocks_x, uint32 blocks_y, uint32 chunks_x, uint32 chunks_y)
      {
         dst_size_in_bytes;

         uint32 chunk_encoding_bits = 1;

         const uint32 num_color_endpoints = m_color_endpoints.size();
         const uint32 num_color_selectors = m_color_selectors.size();
         const uint32 num_alpha_endpoints = m_alpha_endpoints.size();
         const uint32 num_alpha_selectors = m_pHeader->m_alpha_selectors.m_num;

         uint32 prev_color_endpoint_index = 0;
         uint32 prev_color_selector_index = 0;
         uint32 prev_alpha_endpoint_index = 0;
         uint32 prev_alpha_selector_index = 0;

         const uint32 num_faces = m_pHeader->m_faces;

         //const uint32 row_pitch_in_dwords = row_pitch_in_bytes >> 2U;

         const int32 cBytesPerBlock = 16;

         CRND_HUFF_DECODE_BEGIN(m_codec);

         for (uint32 f = 0; f < num_faces; f++)
         {
            uint8* CRND_RESTRICT pRow = pDst[f];

            for (uint32 y = 0; y < chunks_y; y++)
            {
               int32 start_x = 0;
               int32 end_x = chunks_x;
               int32 dir_x = 1;
               int32 block_delta = cBytesPerBlock*2;
               uint8* CRND_RESTRICT pBlock = pRow;

               if (y & 1)
               {
                  start_x = chunks_x - 1;
                  end_x = -1;
                  dir_x = -1;
                  block_delta = -cBytesPerBlock*2;
                  pBlock += (chunks_x - 1) * cBytesPerBlock * 2;
               }

               const bool skip_bottom_row = (y == (chunks_y - 1)) && (blocks_y & 1);

               for (int32 x = start_x; x != end_x; x += dir_x)
               {
                  uint32 color_endpoints[4];
                  uint32 alpha_endpoints[4];

                  if (chunk_encoding_bits == 1)
                  {
                     CRND_HUFF_DECODE(m_codec, m_chunk_encoding_dm, chunk_encoding_bits);
                     chunk_encoding_bits |= 512;
                  }

                  const uint32 chunk_encoding_index = chunk_encoding_bits & 7;
                  chunk_encoding_bits >>= 3;

                  const uint32 num_tiles = g_crnd_chunk_encoding_num_tiles[chunk_encoding_index];

                  const uint8* pTile_indices = g_crnd_chunk_encoding_tiles[chunk_encoding_index].m_tiles;

                  const bool skip_right_col = (blocks_x & 1) && (x == ((int32)chunks_x - 1));

                  uint32* CRND_RESTRICT pD = (uint32*)pBlock;

                  for (uint32 i = 0; i < num_tiles; i++)
                  {
                     uint32 delta; CRND_HUFF_DECODE(m_codec, m_endpoint_delta_dm[1], delta);
                     prev_alpha_endpoint_index += delta;
                     limit(prev_alpha_endpoint_index, num_alpha_endpoints);
                     alpha_endpoints[i] = m_alpha_endpoints[prev_alpha_endpoint_index];
                  }

                  for (uint32 i = 0; i < num_tiles; i++)
                  {
                     uint32 delta; CRND_HUFF_DECODE(m_codec, m_endpoint_delta_dm[0], delta);
                     prev_color_endpoint_index += delta;
                     limit(prev_color_endpoint_index, num_color_endpoints);
                     color_endpoints[i] = m_color_endpoints[prev_color_endpoint_index];
                  }

                  pD = (uint32*)pBlock;
                  for (uint32 by = 0; by < 2; by++)
                  {
                     for (uint32 bx = 0; bx < 2; bx++, pD += 4)
                     {
                        uint32 delta0; CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[1], delta0);
                        prev_alpha_selector_index += delta0;
                        limit(prev_alpha_selector_index, num_alpha_selectors);

                        uint32 delta1; CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[0], delta1);
                        prev_color_selector_index += delta1;
                        limit(prev_color_selector_index, num_color_selectors);

                        if (!((bx && skip_right_col) || (by && skip_bottom_row)))
                        {
                           const uint32 tile_index = pTile_indices[bx + by * 2];
                           const uint16* pAlpha_selectors = &m_alpha_selectors[prev_alpha_selector_index * 3];

#ifdef CRND_BIG_ENDIAN_PLATFORM
                           pD[0] = (alpha_endpoints[tile_index] << 16) | pAlpha_selectors[0];
                           CRND_WRITE_BARRIER
                           pD[1] = (pAlpha_selectors[1] << 16) | pAlpha_selectors[2];
                           CRND_WRITE_BARRIER
                           pD[2] = color_endpoints[tile_index];
                           CRND_WRITE_BARRIER
                           pD[3] = m_color_selectors[prev_color_selector_index];
                           CRND_WRITE_BARRIER
#else
                           pD[0] = alpha_endpoints[tile_index] | (pAlpha_selectors[0] << 16);
                           CRND_WRITE_BARRIER
                           pD[1] = pAlpha_selectors[1] | (pAlpha_selectors[2] << 16);
                           CRND_WRITE_BARRIER
                           pD[2] = color_endpoints[tile_index];
                           CRND_WRITE_BARRIER
                           pD[3] = m_color_selectors[prev_color_selector_index];
                           CRND_WRITE_BARRIER
#endif
                        }
                     }

                     pD = (uint32*)((uint8*)pD - cBytesPerBlock * 2 + row_pitch_in_bytes);
                  }

                  pBlock += block_delta;

               } // x

               pRow += row_pitch_in_bytes * 2;

            } // y

         } // f

         CRND_HUFF_DECODE_END(m_codec);

         return true;
      }

      bool unpack_dxn(uint8** pDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes, uint32 blocks_x, uint32 blocks_y, uint32 chunks_x, uint32 chunks_y)
      {
         dst_size_in_bytes;

         uint32 chunk_encoding_bits = 1;

         const uint32 num_alpha_endpoints = m_alpha_endpoints.size();
         const uint32 num_alpha_selectors = m_pHeader->m_alpha_selectors.m_num;

         uint32 prev_alpha0_endpoint_index = 0;
         uint32 prev_alpha0_selector_index = 0;
         uint32 prev_alpha1_endpoint_index = 0;
         uint32 prev_alpha1_selector_index = 0;

         const uint32 num_faces = m_pHeader->m_faces;

         //const uint32 row_pitch_in_dwords = row_pitch_in_bytes >> 2U;

         const int32 cBytesPerBlock = 16;

         CRND_HUFF_DECODE_BEGIN(m_codec);

         for (uint32 f = 0; f < num_faces; f++)
         {
            uint8* CRND_RESTRICT pRow = pDst[f];

            for (uint32 y = 0; y < chunks_y; y++)
            {
               int32 start_x = 0;
               int32 end_x = chunks_x;
               int32 dir_x = 1;
               int32 block_delta = cBytesPerBlock*2;
               uint8* CRND_RESTRICT pBlock = pRow;

               if (y & 1)
               {
                  start_x = chunks_x - 1;
                  end_x = -1;
                  dir_x = -1;
                  block_delta = -cBytesPerBlock*2;
                  pBlock += (chunks_x - 1) * cBytesPerBlock * 2;
               }

               const bool skip_bottom_row = (y == (chunks_y - 1)) && (blocks_y & 1);

               for (int32 x = start_x; x != end_x; x += dir_x)
               {
                  uint32 alpha0_endpoints[4];
                  uint32 alpha1_endpoints[4];

                  if (chunk_encoding_bits == 1)
                  {
                     CRND_HUFF_DECODE(m_codec, m_chunk_encoding_dm, chunk_encoding_bits);
                     chunk_encoding_bits |= 512;
                  }

                  const uint32 chunk_encoding_index = chunk_encoding_bits & 7;
                  chunk_encoding_bits >>= 3;

                  const uint32 num_tiles = g_crnd_chunk_encoding_num_tiles[chunk_encoding_index];

                  const uint8* pTile_indices = g_crnd_chunk_encoding_tiles[chunk_encoding_index].m_tiles;

                  const bool skip_right_col = (blocks_x & 1) && (x == ((int32)chunks_x - 1));

                  uint32* CRND_RESTRICT pD = (uint32*)pBlock;

                  for (uint32 i = 0; i < num_tiles; i++)
                  {
                     uint32 delta; CRND_HUFF_DECODE(m_codec, m_endpoint_delta_dm[1], delta);
                     prev_alpha0_endpoint_index += delta;
                     limit(prev_alpha0_endpoint_index, num_alpha_endpoints);
                     alpha0_endpoints[i] = m_alpha_endpoints[prev_alpha0_endpoint_index];
                  }

                  for (uint32 i = 0; i < num_tiles; i++)
                  {
                     uint32 delta; CRND_HUFF_DECODE(m_codec, m_endpoint_delta_dm[1], delta);
                     prev_alpha1_endpoint_index += delta;
                     limit(prev_alpha1_endpoint_index, num_alpha_endpoints);
                     alpha1_endpoints[i] = m_alpha_endpoints[prev_alpha1_endpoint_index];
                  }

                  pD = (uint32*)pBlock;
                  for (uint32 by = 0; by < 2; by++)
                  {
                     for (uint32 bx = 0; bx < 2; bx++, pD += 4)
                     {
                        uint32 delta0; CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[1], delta0);
                        prev_alpha0_selector_index += delta0;
                        limit(prev_alpha0_selector_index, num_alpha_selectors);

                        uint32 delta1; CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[1], delta1);
                        prev_alpha1_selector_index += delta1;
                        limit(prev_alpha1_selector_index, num_alpha_selectors);

                        if (!((bx && skip_right_col) || (by && skip_bottom_row)))
                        {
                           const uint32 tile_index = pTile_indices[bx + by * 2];
                           const uint16* pAlpha0_selectors = &m_alpha_selectors[prev_alpha0_selector_index * 3];
                           const uint16* pAlpha1_selectors = &m_alpha_selectors[prev_alpha1_selector_index * 3];

#ifdef CRND_BIG_ENDIAN_PLATFORM
                           pD[0] = (alpha0_endpoints[tile_index] << 16) | pAlpha0_selectors[0];
                           CRND_WRITE_BARRIER
                           pD[1] = (pAlpha0_selectors[1] << 16) | pAlpha0_selectors[2];
                           CRND_WRITE_BARRIER
                           pD[2] = (alpha1_endpoints[tile_index] << 16) | pAlpha1_selectors[0];
                           CRND_WRITE_BARRIER
                           pD[3] = (pAlpha1_selectors[1] << 16) | pAlpha1_selectors[2];
                           CRND_WRITE_BARRIER
#else
                           pD[0] = alpha0_endpoints[tile_index] | (pAlpha0_selectors[0] << 16);
                           CRND_WRITE_BARRIER
                           pD[1] = pAlpha0_selectors[1] | (pAlpha0_selectors[2] << 16);
                           CRND_WRITE_BARRIER
                           pD[2] = alpha1_endpoints[tile_index] | (pAlpha1_selectors[0] << 16);
                           CRND_WRITE_BARRIER
                           pD[3] = pAlpha1_selectors[1] | (pAlpha1_selectors[2] << 16);
                           CRND_WRITE_BARRIER
#endif
                        }
                     }

                     pD = (uint32*)((uint8*)pD - cBytesPerBlock * 2 + row_pitch_in_bytes);
                  }

                  pBlock += block_delta;

               } // x

               pRow += row_pitch_in_bytes * 2;

            } // y

         } // f

         CRND_HUFF_DECODE_END(m_codec);

         return true;
      }

      bool unpack_dxt5a(uint8** pDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes, uint32 blocks_x, uint32 blocks_y, uint32 chunks_x, uint32 chunks_y)
      {
         dst_size_in_bytes;

         uint32 chunk_encoding_bits = 1;

         const uint32 num_alpha_endpoints = m_alpha_endpoints.size();
         const uint32 num_alpha_selectors = m_pHeader->m_alpha_selectors.m_num;

         uint32 prev_alpha0_endpoint_index = 0;
         uint32 prev_alpha0_selector_index = 0;

         const uint32 num_faces = m_pHeader->m_faces;

         const int32 cBytesPerBlock = 8;

         CRND_HUFF_DECODE_BEGIN(m_codec);

         for (uint32 f = 0; f < num_faces; f++)
         {
            uint8* CRND_RESTRICT pRow = pDst[f];

            for (uint32 y = 0; y < chunks_y; y++)
            {
               int32 start_x = 0;
               int32 end_x = chunks_x;
               int32 dir_x = 1;
               int32 block_delta = cBytesPerBlock*2;
               uint8* CRND_RESTRICT pBlock = pRow;

               if (y & 1)
               {
                  start_x = chunks_x - 1;
                  end_x = -1;
                  dir_x = -1;
                  block_delta = -cBytesPerBlock*2;
                  pBlock += (chunks_x - 1) * cBytesPerBlock * 2;
               }

               const bool skip_bottom_row = (y == (chunks_y - 1)) && (blocks_y & 1);

               for (int32 x = start_x; x != end_x; x += dir_x)
               {
                  uint32 alpha0_endpoints[4];

                  if (chunk_encoding_bits == 1)
                  {
                     CRND_HUFF_DECODE(m_codec, m_chunk_encoding_dm, chunk_encoding_bits);
                     chunk_encoding_bits |= 512;
                  }

                  const uint32 chunk_encoding_index = chunk_encoding_bits & 7;
                  chunk_encoding_bits >>= 3;

                  const uint32 num_tiles = g_crnd_chunk_encoding_num_tiles[chunk_encoding_index];

                  const uint8* pTile_indices = g_crnd_chunk_encoding_tiles[chunk_encoding_index].m_tiles;

                  const bool skip_right_col = (blocks_x & 1) && (x == ((int32)chunks_x - 1));

                  uint32* CRND_RESTRICT pD = (uint32*)pBlock;

                  for (uint32 i = 0; i < num_tiles; i++)
                  {
                     uint32 delta; CRND_HUFF_DECODE(m_codec, m_endpoint_delta_dm[1], delta);
                     prev_alpha0_endpoint_index += delta;
                     limit(prev_alpha0_endpoint_index, num_alpha_endpoints);
                     alpha0_endpoints[i] = m_alpha_endpoints[prev_alpha0_endpoint_index];
                  }

                  pD = (uint32*)pBlock;
                  for (uint32 by = 0; by < 2; by++)
                  {
                     for (uint32 bx = 0; bx < 2; bx++, pD += 2)
                     {
                        uint32 delta; CRND_HUFF_DECODE(m_codec, m_selector_delta_dm[1], delta);
                        prev_alpha0_selector_index += delta;
                        limit(prev_alpha0_selector_index, num_alpha_selectors);

                        if (!((bx && skip_right_col) || (by && skip_bottom_row)))
                        {
                           const uint32 tile_index = pTile_indices[bx + by * 2];
                           const uint16* pAlpha0_selectors = &m_alpha_selectors[prev_alpha0_selector_index * 3];

#if CRND_BIG_ENDIAN_PLATFORM
                           pD[0] = (alpha0_endpoints[tile_index] << 16) | pAlpha0_selectors[0];
                           CRND_WRITE_BARRIER
                           pD[1] = (pAlpha0_selectors[1] << 16) | pAlpha0_selectors[2];
                           CRND_WRITE_BARRIER
#else
                           pD[0] = alpha0_endpoints[tile_index] | (pAlpha0_selectors[0] << 16);
                           CRND_WRITE_BARRIER
                           pD[1] = pAlpha0_selectors[1] | (pAlpha0_selectors[2] << 16);
                           CRND_WRITE_BARRIER
#endif
                        }
                     }

                     pD = (uint32*)((uint8*)pD - cBytesPerBlock * 2 + row_pitch_in_bytes);
                  }

                  pBlock += block_delta;

               } // x

               pRow += row_pitch_in_bytes * 2;

            } // y

         } // f

         CRND_HUFF_DECODE_END(m_codec);

         return true;
      }
   };

   crnd_unpack_context crnd_unpack_begin(const void* pData, uint32 data_size)
   {
      if ((!pData) || (data_size < cCRNHeaderMinSize))
         return NULL;

      crn_unpacker* p = crnd_new<crn_unpacker>();
      if (!p)
         return NULL;

      if (!p->init(pData, data_size))
      {
         crnd_delete(p);
         return NULL;
      }

      return p;
   }

   bool crnd_get_data(crnd_unpack_context pContext, const void** ppData, uint32* pData_size)
   {
      if (!pContext)
         return false;

      crn_unpacker* pUnpacker = static_cast<crn_unpacker*>(pContext);

      if (!pUnpacker->is_valid())
         return false;

      if (ppData)
         *ppData = pUnpacker->get_data();

      if (pData_size)
         *pData_size = pUnpacker->get_data_size();

      return true;
   }

   bool crnd_unpack_level(
      crnd_unpack_context pContext,
      void** pDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes,
      uint32 level_index)
   {
      if ((!pContext) || (!pDst) || (dst_size_in_bytes < 8U) || (level_index >= cCRNMaxLevels))
         return false;

      crn_unpacker* pUnpacker = static_cast<crn_unpacker*>(pContext);

      if (!pUnpacker->is_valid())
         return false;

      return pUnpacker->unpack_level(pDst, dst_size_in_bytes, row_pitch_in_bytes, level_index);
   }

   bool crnd_unpack_level_segmented(
      crnd_unpack_context pContext,
      const void* pSrc, uint32 src_size_in_bytes,
      void** pDst, uint32 dst_size_in_bytes, uint32 row_pitch_in_bytes,
      uint32 level_index)
   {
      if ((!pContext) || (!pSrc) || (!pDst) || (dst_size_in_bytes < 8U) || (level_index >= cCRNMaxLevels))
         return false;

      crn_unpacker* pUnpacker = static_cast<crn_unpacker*>(pContext);

      if (!pUnpacker->is_valid())
         return false;

      return pUnpacker->unpack_level(pSrc, src_size_in_bytes, pDst, dst_size_in_bytes, row_pitch_in_bytes, level_index);
   }

   bool crnd_unpack_end(crnd_unpack_context pContext)
   {
      if (!pContext)
         return false;

      crn_unpacker* pUnpacker = static_cast<crn_unpacker*>(pContext);

      if (!pUnpacker->is_valid())
         return false;

      crnd_delete(pUnpacker);

      return true;
   }

} // namespace crnd

#endif // CRND_HEADER_FILE_ONLY

  //------------------------------------------------------------------------------
  //
  // crunch/crnlib uses a modified ZLIB license. Specifically, it's the same as zlib except that 
  // public credits for using the library are *required*.
  // 
  // Copyright (c) 2010-2016 Richard Geldreich, Jr. All rights reserved.
  //
  // This software is provided 'as-is', without any express or implied
  // warranty.  In no event will the authors be held liable for any damages
  // arising from the use of this software.
  // 
  // Permission is granted to anyone to use this software for any purpose,
  // including commercial applications, and to alter it and redistribute it
  // freely, subject to the following restrictions:
  // 
  // 1. The origin of this software must not be misrepresented; you must not
  // claim that you wrote the original software. 
  // 
  // 2. If you use this software in a product, this acknowledgment in the product 
  // documentation or credits is required:
  // 
  // "Crunch Library Copyright (c) 2010-2016 Richard Geldreich, Jr."
  // 
  // 3. Altered source versions must be plainly marked as such, and must not be
  // misrepresented as being the original software.
  // 
  // 4. This notice may not be removed or altered from any source distribution.
  //
  //------------------------------------------------------------------------------

