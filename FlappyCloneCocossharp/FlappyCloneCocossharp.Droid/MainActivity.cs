using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using CocosSharp;

namespace FlappyCloneCocossharp.Droid
{
	[Activity (Label = "Flappy Clone", 
               AlwaysRetainTaskState = true,
               Icon = "@drawable/icon", 
               ScreenOrientation = ScreenOrientation.Portrait,
               LaunchMode = LaunchMode.SingleInstance,
               MainLauncher = true, 
               ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

            int newUiOptions = (int)Window.DecorView.SystemUiVisibility;

            newUiOptions |= (int)SystemUiFlags.LowProfile;
            newUiOptions |= (int)SystemUiFlags.Fullscreen;
            newUiOptions |= (int)SystemUiFlags.HideNavigation;
            newUiOptions |= (int)SystemUiFlags.ImmersiveSticky;

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

            global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new FlappyCloneCocossharp.App ());
		}
	}
}

