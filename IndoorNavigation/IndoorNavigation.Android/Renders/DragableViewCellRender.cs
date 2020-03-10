using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using IndoorNavigation.Droid.Custom;
using IndoorNavigation.Droid.Renders;
using IndoorNavigation.Views.Controls;
[assembly: ExportRenderer(typeof(DraggableViewCell),typeof(DragAndDropViewCellRenderer))]
namespace IndoorNavigation.Droid.Renders
{
	public class DragAndDropViewCellRenderer : ViewCellRenderer
	{
		protected override global::Android.Views.View GetCellCore(Xamarin.Forms.Cell item, global::Android.Views.View convertView, global::Android.Views.ViewGroup parent, global::Android.Content.Context context)
		{
			var cell = base.GetCellCore(item, convertView, parent, context) as ViewGroup;

			cell.SetOnDragListener(new ItemDragListener(cell));
			return cell;
		}
	}

}