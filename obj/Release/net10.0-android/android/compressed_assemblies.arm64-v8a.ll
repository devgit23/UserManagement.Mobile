; ModuleID = 'compressed_assemblies.arm64-v8a.ll'
source_filename = "compressed_assemblies.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i1, ; bool loaded
	i32 ; uint32_t buffer_offset
}

@compressed_assembly_count = dso_local local_unnamed_addr constant i32 144, align 4

@compressed_assembly_descriptors = dso_local local_unnamed_addr global [144 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 0; uint32_t buffer_offset
	}, ; 0: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 15624; uint32_t buffer_offset
	}, ; 1: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 31256; uint32_t buffer_offset
	}, ; 2: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 46880; uint32_t buffer_offset
	}, ; 3: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 62504; uint32_t buffer_offset
	}, ; 4: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 78136; uint32_t buffer_offset
	}, ; 5: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 93768; uint32_t buffer_offset
	}, ; 6: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 109400; uint32_t buffer_offset
	}, ; 7: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 125024; uint32_t buffer_offset
	}, ; 8: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 140648; uint32_t buffer_offset
	}, ; 9: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 156280; uint32_t buffer_offset
	}, ; 10: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 171904; uint32_t buffer_offset
	}, ; 11: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 187528; uint32_t buffer_offset
	}, ; 12: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 203152; uint32_t buffer_offset
	}, ; 13: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 218776; uint32_t buffer_offset
	}, ; 14: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 234400; uint32_t buffer_offset
	}, ; 15: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 250024; uint32_t buffer_offset
	}, ; 16: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 265648; uint32_t buffer_offset
	}, ; 17: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 281272; uint32_t buffer_offset
	}, ; 18: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 296904; uint32_t buffer_offset
	}, ; 19: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 312568; uint32_t buffer_offset
	}, ; 20: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 328192; uint32_t buffer_offset
	}, ; 21: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 343824; uint32_t buffer_offset
	}, ; 22: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 359456; uint32_t buffer_offset
	}, ; 23: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 375088; uint32_t buffer_offset
	}, ; 24: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 390760; uint32_t buffer_offset
	}, ; 25: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 406392; uint32_t buffer_offset
	}, ; 26: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 422056; uint32_t buffer_offset
	}, ; 27: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 437680; uint32_t buffer_offset
	}, ; 28: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 453304; uint32_t buffer_offset
	}, ; 29: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 468928; uint32_t buffer_offset
	}, ; 30: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15664, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 484552; uint32_t buffer_offset
	}, ; 31: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 500216; uint32_t buffer_offset
	}, ; 32: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 15632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 515840; uint32_t buffer_offset
	}, ; 33: Microsoft.Maui.Controls.resources
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 531472; uint32_t buffer_offset
	}, ; 34: _Microsoft.Android.Resource.Designer
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 537616; uint32_t buffer_offset
	}, ; 35: CommunityToolkit.Mvvm
	%struct.CompressedAssemblyDescriptor {
		i32 19968, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 556048; uint32_t buffer_offset
	}, ; 36: Microsoft.Extensions.Configuration
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 576016; uint32_t buffer_offset
	}, ; 37: Microsoft.Extensions.Configuration.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 583184; uint32_t buffer_offset
	}, ; 38: Microsoft.Extensions.Configuration.Json
	%struct.CompressedAssemblyDescriptor {
		i32 46592, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 593936; uint32_t buffer_offset
	}, ; 39: Microsoft.Extensions.DependencyInjection
	%struct.CompressedAssemblyDescriptor {
		i32 27136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 640528; uint32_t buffer_offset
	}, ; 40: Microsoft.Extensions.DependencyInjection.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 667664; uint32_t buffer_offset
	}, ; 41: Microsoft.Extensions.Diagnostics
	%struct.CompressedAssemblyDescriptor {
		i32 8704, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 682512; uint32_t buffer_offset
	}, ; 42: Microsoft.Extensions.Diagnostics.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 691216; uint32_t buffer_offset
	}, ; 43: Microsoft.Extensions.FileProviders.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 698384; uint32_t buffer_offset
	}, ; 44: Microsoft.Extensions.Hosting.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 704528; uint32_t buffer_offset
	}, ; 45: Microsoft.Extensions.Http
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 744976; uint32_t buffer_offset
	}, ; 46: Microsoft.Extensions.Logging
	%struct.CompressedAssemblyDescriptor {
		i32 28160, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 764432; uint32_t buffer_offset
	}, ; 47: Microsoft.Extensions.Logging.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 20992, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 792592; uint32_t buffer_offset
	}, ; 48: Microsoft.Extensions.Options
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 813584; uint32_t buffer_offset
	}, ; 49: Microsoft.Extensions.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 1928504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 822800; uint32_t buffer_offset
	}, ; 50: Microsoft.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 135432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2751304; uint32_t buffer_offset
	}, ; 51: Microsoft.Maui.Controls.Xaml
	%struct.CompressedAssemblyDescriptor {
		i32 861696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2886736; uint32_t buffer_offset
	}, ; 52: Microsoft.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 65024, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3748432; uint32_t buffer_offset
	}, ; 53: Microsoft.Maui.Essentials
	%struct.CompressedAssemblyDescriptor {
		i32 208696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3813456; uint32_t buffer_offset
	}, ; 54: Microsoft.Maui.Graphics
	%struct.CompressedAssemblyDescriptor {
		i32 153040, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4022152; uint32_t buffer_offset
	}, ; 55: Refit
	%struct.CompressedAssemblyDescriptor {
		i32 35256, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4175192; uint32_t buffer_offset
	}, ; 56: Refit.HttpClientFactory
	%struct.CompressedAssemblyDescriptor {
		i32 107520, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4210448; uint32_t buffer_offset
	}, ; 57: SQLite-net
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4317968; uint32_t buffer_offset
	}, ; 58: SQLitePCLRaw.batteries_v2
	%struct.CompressedAssemblyDescriptor {
		i32 51200, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4323600; uint32_t buffer_offset
	}, ; 59: SQLitePCLRaw.core
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4374800; uint32_t buffer_offset
	}, ; 60: SQLitePCLRaw.lib.e_sqlite3.android
	%struct.CompressedAssemblyDescriptor {
		i32 36864, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4379920; uint32_t buffer_offset
	}, ; 61: SQLitePCLRaw.provider.e_sqlite3
	%struct.CompressedAssemblyDescriptor {
		i32 73728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4416784; uint32_t buffer_offset
	}, ; 62: Xamarin.AndroidX.Activity
	%struct.CompressedAssemblyDescriptor {
		i32 583680, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 4490512; uint32_t buffer_offset
	}, ; 63: Xamarin.AndroidX.AppCompat
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5074192; uint32_t buffer_offset
	}, ; 64: Xamarin.AndroidX.AppCompat.AppCompatResources
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5092112; uint32_t buffer_offset
	}, ; 65: Xamarin.AndroidX.CardView
	%struct.CompressedAssemblyDescriptor {
		i32 22528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5111056; uint32_t buffer_offset
	}, ; 66: Xamarin.AndroidX.Collection.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 78336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5133584; uint32_t buffer_offset
	}, ; 67: Xamarin.AndroidX.CoordinatorLayout
	%struct.CompressedAssemblyDescriptor {
		i32 596992, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5211920; uint32_t buffer_offset
	}, ; 68: Xamarin.AndroidX.Core
	%struct.CompressedAssemblyDescriptor {
		i32 26624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5808912; uint32_t buffer_offset
	}, ; 69: Xamarin.AndroidX.CursorAdapter
	%struct.CompressedAssemblyDescriptor {
		i32 9728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5835536; uint32_t buffer_offset
	}, ; 70: Xamarin.AndroidX.CustomView
	%struct.CompressedAssemblyDescriptor {
		i32 47104, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5845264; uint32_t buffer_offset
	}, ; 71: Xamarin.AndroidX.DrawerLayout
	%struct.CompressedAssemblyDescriptor {
		i32 236032, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 5892368; uint32_t buffer_offset
	}, ; 72: Xamarin.AndroidX.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 23552, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6128400; uint32_t buffer_offset
	}, ; 73: Xamarin.AndroidX.Lifecycle.Common.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 18944, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6151952; uint32_t buffer_offset
	}, ; 74: Xamarin.AndroidX.Lifecycle.LiveData.Core
	%struct.CompressedAssemblyDescriptor {
		i32 32768, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6170896; uint32_t buffer_offset
	}, ; 75: Xamarin.AndroidX.Lifecycle.ViewModel.Android
	%struct.CompressedAssemblyDescriptor {
		i32 13824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6203664; uint32_t buffer_offset
	}, ; 76: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 39424, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6217488; uint32_t buffer_offset
	}, ; 77: Xamarin.AndroidX.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 92672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6256912; uint32_t buffer_offset
	}, ; 78: Xamarin.AndroidX.Navigation.Common.Android
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6349584; uint32_t buffer_offset
	}, ; 79: Xamarin.AndroidX.Navigation.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 65536, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6369040; uint32_t buffer_offset
	}, ; 80: Xamarin.AndroidX.Navigation.Runtime.Android
	%struct.CompressedAssemblyDescriptor {
		i32 27136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6434576; uint32_t buffer_offset
	}, ; 81: Xamarin.AndroidX.Navigation.UI
	%struct.CompressedAssemblyDescriptor {
		i32 457728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6461712; uint32_t buffer_offset
	}, ; 82: Xamarin.AndroidX.RecyclerView
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6919440; uint32_t buffer_offset
	}, ; 83: Xamarin.AndroidX.SavedState.SavedState.Android
	%struct.CompressedAssemblyDescriptor {
		i32 24576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6931728; uint32_t buffer_offset
	}, ; 84: Xamarin.AndroidX.Security.SecurityCrypto
	%struct.CompressedAssemblyDescriptor {
		i32 41984, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6956304; uint32_t buffer_offset
	}, ; 85: Xamarin.AndroidX.SwipeRefreshLayout
	%struct.CompressedAssemblyDescriptor {
		i32 62976, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 6998288; uint32_t buffer_offset
	}, ; 86: Xamarin.AndroidX.ViewPager
	%struct.CompressedAssemblyDescriptor {
		i32 40448, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7061264; uint32_t buffer_offset
	}, ; 87: Xamarin.AndroidX.ViewPager2
	%struct.CompressedAssemblyDescriptor {
		i32 675840, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7101712; uint32_t buffer_offset
	}, ; 88: Xamarin.Google.Android.Material
	%struct.CompressedAssemblyDescriptor {
		i32 345088, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 7777552; uint32_t buffer_offset
	}, ; 89: Xamarin.Google.Crypto.Tink.Android
	%struct.CompressedAssemblyDescriptor {
		i32 90624, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8122640; uint32_t buffer_offset
	}, ; 90: Xamarin.Kotlin.StdLib
	%struct.CompressedAssemblyDescriptor {
		i32 28672, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8213264; uint32_t buffer_offset
	}, ; 91: Xamarin.KotlinX.Coroutines.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 91648, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8241936; uint32_t buffer_offset
	}, ; 92: Xamarin.KotlinX.Serialization.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 461312, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8333584; uint32_t buffer_offset
	}, ; 93: UserManagement.Common
	%struct.CompressedAssemblyDescriptor {
		i32 117248, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8794896; uint32_t buffer_offset
	}, ; 94: UserManagement.Mobile.Core
	%struct.CompressedAssemblyDescriptor {
		i32 132608, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 8912144; uint32_t buffer_offset
	}, ; 95: UserManagement.Shared
	%struct.CompressedAssemblyDescriptor {
		i32 328704, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9044752; uint32_t buffer_offset
	}, ; 96: UserManagement.Mobile
	%struct.CompressedAssemblyDescriptor {
		i32 24576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9373456; uint32_t buffer_offset
	}, ; 97: System.Collections.Concurrent
	%struct.CompressedAssemblyDescriptor {
		i32 15872, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9398032; uint32_t buffer_offset
	}, ; 98: System.Collections.NonGeneric
	%struct.CompressedAssemblyDescriptor {
		i32 13824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9413904; uint32_t buffer_offset
	}, ; 99: System.Collections.Specialized
	%struct.CompressedAssemblyDescriptor {
		i32 31232, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9427728; uint32_t buffer_offset
	}, ; 100: System.Collections
	%struct.CompressedAssemblyDescriptor {
		i32 31744, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9458960; uint32_t buffer_offset
	}, ; 101: System.ComponentModel.Annotations
	%struct.CompressedAssemblyDescriptor {
		i32 12800, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9490704; uint32_t buffer_offset
	}, ; 102: System.ComponentModel.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 110592, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9503504; uint32_t buffer_offset
	}, ; 103: System.ComponentModel.TypeConverter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9614096; uint32_t buffer_offset
	}, ; 104: System.ComponentModel
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9619216; uint32_t buffer_offset
	}, ; 105: System.Console
	%struct.CompressedAssemblyDescriptor {
		i32 64512, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9631504; uint32_t buffer_offset
	}, ; 106: System.Diagnostics.DiagnosticSource
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9696016; uint32_t buffer_offset
	}, ; 107: System.Drawing.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9707792; uint32_t buffer_offset
	}, ; 108: System.Drawing
	%struct.CompressedAssemblyDescriptor {
		i32 60416, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9712912; uint32_t buffer_offset
	}, ; 109: System.Formats.Asn1
	%struct.CompressedAssemblyDescriptor {
		i32 22016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9773328; uint32_t buffer_offset
	}, ; 110: System.IO.Compression.Brotli
	%struct.CompressedAssemblyDescriptor {
		i32 31744, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9795344; uint32_t buffer_offset
	}, ; 111: System.IO.Compression
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9827088; uint32_t buffer_offset
	}, ; 112: System.IO.Pipelines
	%struct.CompressedAssemblyDescriptor {
		i32 356864, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 9833744; uint32_t buffer_offset
	}, ; 113: System.Linq.Expressions
	%struct.CompressedAssemblyDescriptor {
		i32 65024, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10190608; uint32_t buffer_offset
	}, ; 114: System.Linq
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10255632; uint32_t buffer_offset
	}, ; 115: System.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10269968; uint32_t buffer_offset
	}, ; 116: System.Net.Http.Json
	%struct.CompressedAssemblyDescriptor {
		i32 150016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10284816; uint32_t buffer_offset
	}, ; 117: System.Net.Http
	%struct.CompressedAssemblyDescriptor {
		i32 38912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10434832; uint32_t buffer_offset
	}, ; 118: System.Net.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10473744; uint32_t buffer_offset
	}, ; 119: System.Net.Requests
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10480912; uint32_t buffer_offset
	}, ; 120: System.Numerics.Vectors
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10486032; uint32_t buffer_offset
	}, ; 121: System.ObjectModel
	%struct.CompressedAssemblyDescriptor {
		i32 73728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10504464; uint32_t buffer_offset
	}, ; 122: System.Private.Uri
	%struct.CompressedAssemblyDescriptor {
		i32 396288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10578192; uint32_t buffer_offset
	}, ; 123: System.Private.Xml
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10974480; uint32_t buffer_offset
	}, ; 124: System.Runtime.InteropServices.RuntimeInformation
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10979600; uint32_t buffer_offset
	}, ; 125: System.Runtime.InteropServices
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10988816; uint32_t buffer_offset
	}, ; 126: System.Runtime.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 77824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 10993936; uint32_t buffer_offset
	}, ; 127: System.Runtime.Numerics
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11071760; uint32_t buffer_offset
	}, ; 128: System.Runtime.Serialization.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11077392; uint32_t buffer_offset
	}, ; 129: System.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 123904, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11092240; uint32_t buffer_offset
	}, ; 130: System.Security.Cryptography
	%struct.CompressedAssemblyDescriptor {
		i32 29696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11216144; uint32_t buffer_offset
	}, ; 131: System.Text.Encodings.Web
	%struct.CompressedAssemblyDescriptor {
		i32 389120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11245840; uint32_t buffer_offset
	}, ; 132: System.Text.Json
	%struct.CompressedAssemblyDescriptor {
		i32 333312, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11634960; uint32_t buffer_offset
	}, ; 133: System.Text.RegularExpressions
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11968272; uint32_t buffer_offset
	}, ; 134: System.Threading.Thread
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11973392; uint32_t buffer_offset
	}, ; 135: System.Threading
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11985680; uint32_t buffer_offset
	}, ; 136: System.Web.HttpUtility
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 11996432; uint32_t buffer_offset
	}, ; 137: System.Xml.ReaderWriter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12001552; uint32_t buffer_offset
	}, ; 138: System
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12006672; uint32_t buffer_offset
	}, ; 139: netstandard
	%struct.CompressedAssemblyDescriptor {
		i32 2022912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 12015888; uint32_t buffer_offset
	}, ; 140: System.Private.CoreLib
	%struct.CompressedAssemblyDescriptor {
		i32 171008, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 14038800; uint32_t buffer_offset
	}, ; 141: Java.Interop
	%struct.CompressedAssemblyDescriptor {
		i32 22368, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 14209808; uint32_t buffer_offset
	}, ; 142: Mono.Android.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 2056192, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 14232176; uint32_t buffer_offset
	} ; 143: Mono.Android
], align 4

@uncompressed_assemblies_data_size = dso_local local_unnamed_addr constant i32 16288368, align 4

@uncompressed_assemblies_data_buffer = dso_local local_unnamed_addr global [16288368 x i8] zeroinitializer, align 1

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/10.0.1xx @ d549e1dc4e2a083b08b4f24cb5495e81b99d79b5"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
