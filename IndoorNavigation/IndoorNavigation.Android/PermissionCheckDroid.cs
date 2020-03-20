#if __ANDROID_29__
using AndroidX.Core.App;
using AndroidX.Core.Content;
#else
using Android.Support.V4.App;
using Android.Support.V4.Content;
using IndoorNavigation.Models;
#endif
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


//[assembly: Xamarin.Forms.Dependency(typeof(PermissioCheck))]
namespace IndoorNavigation.Droid
{
 //   public class PermissionCheckDroid:  PermissioCheck
 //   {
 //     //public static PermissionCheckDroid Current=>(PermissionCheckDroid)

 //       public Task<bool> ShouldShowRequestPermissionRationaleAsync(Permission permission)
 //       {
	//		var activity = Xamarin.Essentials.Platform.CurrentActivity;
	//		if (activity == null)
	//		{
	//			Debug.WriteLine("Unable to detect current Activity. Please ensure Xamarin.Essentials is installed in your Android project and is initialized.");
	//			return Task.FromResult(false);
	//		}

	//		var names = GetManifestNames(permission);

	//		//if isn't an android specific group then go ahead and return false;
	//		if (names == null)
	//		{
	//			Debug.WriteLine("No android specific permissions needed for: " + permission);
	//			return Task.FromResult(false);
	//		}

	//		if (names.Count == 0)
	//		{
	//			Debug.WriteLine("No permissions found in manifest for: " + permission + " no need to show request rationale");
	//			return Task.FromResult(false);
	//		}

	//		foreach (var name in names)
	//		{
	//			if (ActivityCompat.ShouldShowRequestPermissionRationale(activity, name))
	//				return Task.FromResult(true);
	//		}

	//		return Task.FromResult(false);

	//	}
	//}
}