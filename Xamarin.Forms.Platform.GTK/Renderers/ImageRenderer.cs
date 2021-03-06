﻿using Gdk;
using P8Xamarin.Controls;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
	public class C_ImageControl : Controls.ImageControl, IGTKNativeView
	{
		public IGTKNativeView Control { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public AccessibleDesc C_Accessible { get => new AccessibleDesc(); set => throw new NotImplementedException(); }

		//AccessibleDesc Accessible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		Image control;
		public  C_ImageControl(Image control)
		{
			this.control = control;
		}
		public void Add(GtkFormsContainer container)
		{
			throw new NotImplementedException();
		}

		public SizeRequest GetDesiredSize(double width, double height)
		{
			if(control.WidthRequest <= 1 || control.MinimumHeightRequest <= 1)
				return new SizeRequest(new Size(double.MaxValue, double.MaxValue));
			else
			return new SizeRequest(new Size(control.WidthRequest, control.HeightRequest));
		}

		public void RemoveFromContainer(GtkFormsContainer container)
		{
			throw new NotImplementedException();
		}

		public void ResetBorderColor()
		{
			throw new NotImplementedException();
		}

		public void ResetColor()
		{
			throw new NotImplementedException();
		}

		public void SetBackgroundColor(Color backgroundColor)
		{
			throw new NotImplementedException();
		}

		public void SetBorderColor(Gdk.Color? color)
		{
			throw new NotImplementedException();
		}

		public void SetBorderWidth(uint borderWidth)
		{
			throw new NotImplementedException();
		}

		public void Start()
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		public void UpdateBorderRadius(int topLeft, int topRight, int bottomLeft, int bottomRight)
		{
			throw new NotImplementedException();
		}

		public void UpdateBorderRadius()
		{
			throw new NotImplementedException();
		}

		public void UpdateColor(Color color)
		{
			throw new NotImplementedException();
		}

		public void UpdateSize(int height, int width)
		{
			throw new NotImplementedException();
		}
	}
	public class ImageRenderer : ViewRenderer<Image, C_ImageControl>
	{
		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.Dispose();
					Control = null;
				}
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			//if(e.NewElement == P8Uikernel. P8Image)
			if (Control == null)
			{
				var image = new C_ImageControl(e.NewElement);
				SetNativeControl(image);
				P8TemplateLayout.AddView(e.NewElement as Image);
			}

			if (e.NewElement != null)
			{
				SetImage(e.OldElement);
				SetAspect();
				SetOpacity();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				SetImage();
			else if (e.PropertyName == Image.IsOpaqueProperty.PropertyName)
				SetOpacity();
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				SetAspect();
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			Control.SetSizeRequest(allocation.Width, allocation.Height);
		}

		async void SetImage(Image oldElement = null)
		{
			var source = Element.Source;

			if (oldElement != null)
			{
				var oldSource = oldElement.Source;
				if (Equals(oldSource, source))
					return;

				if (oldSource is FileImageSource && source is FileImageSource
					&& ((FileImageSource)oldSource).File == ((FileImageSource)source).File)
					return;

				Control.Pixbuf = null;
			}

			((IImageController)Element).SetIsLoading(true);

			var image = await source.GetNativeImageAsync();

			var imageView = Control;
			if (imageView != null)
				imageView.Pixbuf = image;

			if (!_isDisposed)
			{
				((IVisualElementController)Element).NativeSizeChanged();
				((IImageController)Element).SetIsLoading(false);
			}
		}

		void SetAspect()
		{
			switch (Element.Aspect)
			{
				case Aspect.AspectFit:
					Control.Aspect = Controls.ImageAspect.AspectFit;
					break;
				case Aspect.AspectFill:
					Control.Aspect = Controls.ImageAspect.AspectFill;
					break;
				case Aspect.Fill:
					Control.Aspect = Controls.ImageAspect.Fill;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Element.Aspect));
			}
		}

		void SetOpacity()
		{
			var opacity = Element.Opacity;

			Control.SetAlpha(opacity);
		}
	}

	public interface IImageSourceHandler : IRegisterable
	{
		Task<Pixbuf> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken =
			default(CancellationToken), float scale = 1);
	}

	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public Task<Pixbuf> LoadImageAsync(
			ImageSource imagesource, 
			CancellationToken cancelationToken = default(CancellationToken), 
			float scale = 1f)
		{
			Pixbuf image = null;
			var filesource = imagesource as FileImageSource;

			if (filesource != null)
			{
				var file = filesource.File;
				if (!string.IsNullOrEmpty(file))
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);

					if (File.Exists(imagePath))
					{
						image = new Pixbuf(imagePath);
					}
				}
			}

			return Task.FromResult(image);
		}
	}

	public sealed class StreamImagesourceHandler : IImageSourceHandler
	{
		public async Task<Pixbuf> LoadImageAsync(ImageSource imagesource, CancellationToken cancelationToken = default(CancellationToken), float scale = 1)
		{
			Pixbuf image = null;

			var streamsource = imagesource as StreamImageSource;
			if (streamsource?.Stream == null) return null;
			using (
				var streamImage = await((IStreamImageSource)streamsource)
				.GetStreamAsync(cancelationToken).ConfigureAwait(false))
			{
				if (streamImage != null)
					image = new Pixbuf(streamImage);
			}

			return image;
		}
	}

	public sealed class UriImageSourceHandler : IImageSourceHandler
	{
		public async Task<Pixbuf> LoadImageAsync(
			ImageSource imagesource,
			CancellationToken cancelationToken = default(CancellationToken),
			float scale = 1)
		{
			Pixbuf image = null;

			var imageLoader = imagesource as UriImageSource;

			if (imageLoader?.Uri == null)
				return null;

			using (Stream streamImage = await imageLoader.GetStreamAsync(cancelationToken))
			{
				if (streamImage == null || !streamImage.CanRead)
				{
					return null;
				}

				image = new Pixbuf(streamImage);
			}

			return image;
		}
	}
}
