using Avalonia;
using Avalonia.Controls;
using System.IO;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CCenter.Platform;
using CC.Compat;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace CC.UControl;

public partial class UserControl_profile : UserControl
{

	private string _imageFileName = "";
	public string ImageFileName
	{
		get
		{
			// WPF returned BitmapImage.UriSource.ToString(); stored path / pack uri of the default image
			return _imageFileName;
		}
		set
		{
			LoadImage(value);
		}
	}

	public UserControl_profile()
	{
		InitializeComponent();
	}

	private void LoadImage(string filename)
	{
		if (filename == null || !File.Exists(filename) || filename == "")
		{
			image_profile.Source = Img.Load("Image/Page_FlexiKey/profile/btn_profile_normal.png");
			_imageFileName = "/CC;component/Image/Page_FlexiKey/profile/btn_profile_normal.png";
		}
		else if (filename.Contains(".png") || filename.Contains(".jpg") || filename.Contains(".bmp") || filename.Contains(".gif"))
		{
			using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
				image_profile.Source = new Bitmap(fs);
			_imageFileName = filename;
		}
		else
		{
			image_profile.Source = Img.Load("Image/Page_FlexiKey/profile/btn_profile_normal.png");
			_imageFileName = filename;
		}
	}
}
