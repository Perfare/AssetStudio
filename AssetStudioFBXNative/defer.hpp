#pragma once

namespace _assetstudio
{
	namespace _utilities
	{

		template<typename TFunction>
		struct defer final
		{

		private:

			TFunction _function;
			bool _shouldInvoke;

		public:

			explicit defer(const TFunction& function) noexcept
				: _function(function), _shouldInvoke(true)
			{
			}

			defer(defer<TFunction>&& other) noexcept
				: _function(other._function), _shouldInvoke(other._shouldInvoke)
			{
				other._shouldInvoke = false;
			}

			defer(const defer<TFunction>& other) = delete;

			defer<TFunction>& operator=(defer<TFunction>&& other) noexcept = default;

			defer<TFunction>& operator=(const defer<TFunction>& other) = delete;

			~defer() noexcept
			{
				if (_shouldInvoke)
				{
					_function();
				}
			}

		};

		template<typename TFunction>
		defer<TFunction> deferrer(TFunction fn)
		{
			return defer<TFunction>(fn);
		}

		// Usage:
		// auto_defer([&]() {
		//     ...
		// });
#define _CONCAT_0(x, y) x##y
#define _CONCAT_1(x, y) _CONCAT_0(x, y)
#define auto_defer auto _CONCAT_1(_deferred_block_, _CONCAT(_CONCAT(__COUNTER__, _),__LINE__)) = ::_assetstudio::_utilities::deferrer

	}
}
