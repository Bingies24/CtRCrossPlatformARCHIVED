using System;
using System.Diagnostics; // REMOVE LATER
using System.Drawing;
using System.Runtime.InteropServices;
using CutTheRope.iframework.core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.windows
{
	internal class ScreenSizeManager
	{
		public const int MIN_WINDOW_WIDTH = 200;

		public const int MIN_WINDOW_HEIGHT = 200;

		private bool _isFullScreen;

		private Microsoft.Xna.Framework.Rectangle _windowRect;

		private Microsoft.Xna.Framework.Rectangle _fullScreenRect;

		private int _gameWidth;

		private int _gameHeight;

		private double _gameAspectRatio;

		private Microsoft.Xna.Framework.Rectangle _scaledViewRect;

		private bool _skipChanges;

		public static int MAX_WINDOW_WIDTH
		{
			get
			{
				if (Global.GraphicsDeviceManager.GraphicsProfile == GraphicsProfile.HiDef)
				{
					return 4096;
				}
				return 2048;
			}
		}

		public static int MAX_WINDOW_HEIGHT
		{
			get
			{
				if (Global.GraphicsDeviceManager.GraphicsProfile == GraphicsProfile.HiDef)
				{
					return 4096;
				}
				return 2048;
			}
		}

        public int WindowWidth
		{
			get
			{
				return _windowRect.Width;
			}
		}

		public int WindowHeight
		{
			get
			{
				return _windowRect.Height;
			}
		}

		public int ScreenWidth
		{
			get
			{
				return _fullScreenRect.Width;
			}
		}

		public int ScreenHeight
		{
			get
			{
				return _fullScreenRect.Height;
			}
		}

		public bool IsFullScreen
		{
			get
			{
				return _isFullScreen;
			}
		}

		public Microsoft.Xna.Framework.Rectangle CurrentSize
		{
			get
			{
				if (IsFullScreen)
				{
					return _fullScreenRect;
				}
				return _windowRect;
			}
		}

		public int GameWidth
		{
			get
			{
				return _gameWidth;
			}
		}

		public int GameHeight
		{
			get
			{
				return _gameHeight;
			}
		}

		public Microsoft.Xna.Framework.Rectangle ScaledViewRect
		{
			get
			{
				return _scaledViewRect;
			}
		}

		public bool SkipSizeChanges
		{
			get
			{
				return _skipChanges;
			}
		}

		public double WidthAspectRatio
		{
			get
			{
				return (double)_scaledViewRect.Width / (double)_gameWidth;
			}
		}

		public int TransformWindowToViewX(int x)
		{
			return x - _scaledViewRect.X;
		}

		public int TransformWindowToViewY(int y)
		{
			return y - _scaledViewRect.Y;
		}

		public float TransformViewToGameX(float x)
		{
			return x * (float)_gameWidth / (float)_scaledViewRect.Width;
		}

		public float TransformViewToGameY(float y)
		{
			return y * (float)_gameHeight / (float)_scaledViewRect.Height;
		}

		public ScreenSizeManager(int gameWidth, int gameHeight)
		{
			_gameWidth = gameWidth;
			_gameHeight = gameHeight;
			_gameAspectRatio = (double)gameHeight / (double)gameWidth;
		}

		public void Init(DisplayMode displayMode, int windowWidth, int windowHeight, bool isFullScreen)
		{
			FullScreenRectChanged(displayMode);
			int width = ((windowWidth > 0) ? windowWidth : (displayMode.Width - 100));
			int height = ((windowHeight > 0) ? windowHeight : (displayMode.Height - 100));
			if (width < MIN_WINDOW_WIDTH)
			{
				width = MIN_WINDOW_WIDTH;
			}
			if (width > MAX_WINDOW_WIDTH)
			{
				width = MAX_WINDOW_WIDTH;
			}
			if (width > displayMode.Width)
			{
				width = displayMode.Width;
			}
			if (height < MIN_WINDOW_HEIGHT)
			{
				height = MIN_WINDOW_HEIGHT;
			}
			if (height > MAX_WINDOW_HEIGHT)
			{
				height = MAX_WINDOW_HEIGHT;
			}
			if (height > displayMode.Height)
			{
				height = displayMode.Height;
			}
			WindowRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, width, height));
			if (isFullScreen)
			{
				ToggleFullScreen();
			}
			else
			{
				ApplyWindowSize(WindowWidth, WindowHeight);
			}
		}

		public int ScaledGameWidth(int scaledHeight)
		{
			return (int)((double)scaledHeight / _gameAspectRatio + 0.5);
		}

		public int ScaledGameHeight(int scaledWidth)
		{
			return (int)((double)scaledWidth * _gameAspectRatio + 0.5);
		}

		private void UpdateScaledView()
		{
			if (_skipChanges)
			{
				return;
			}
			Microsoft.Xna.Framework.Rectangle baseRect = _windowRect;
			if (_isFullScreen)
			{
				baseRect = _fullScreenRect;
			}
			int height = baseRect.Height;
			int width = ScaledGameWidth(height);
			_scaledViewRect = new Microsoft.Xna.Framework.Rectangle((baseRect.Width - width) / 2, (baseRect.Height - height) / 2, width, height);
			Debug.WriteLine($"Fullscreen: {IsFullScreen}"); // REMOVE LATER
			Debug.WriteLine($"Base X: {baseRect.X}, Base Y: {baseRect.Y}, Base Width: {baseRect.Width}, Base Height: {baseRect.Height}"); // REMOVE LATER
			Debug.WriteLine($"Scaled X: {_scaledViewRect.X}, Scaled Y: {_scaledViewRect.Y}, Scaled Width: {_scaledViewRect.Width}, Scaled Height: {_scaledViewRect.Height}"); // REMOVE LATER
		}

		public void ApplyWindowSize(int width, int height)
		{
			GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
			graphicsDeviceManager.PreferredBackBufferWidth = width;
			graphicsDeviceManager.PreferredBackBufferHeight = height;
			graphicsDeviceManager.ApplyChanges();
			WindowRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, graphicsDeviceManager.PreferredBackBufferWidth, graphicsDeviceManager.PreferredBackBufferHeight));
		}

		public void ToggleFullScreen()
		{
			_skipChanges = true;
			GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
			int preferredBackBufferWidth = (IsFullScreen ? _windowRect.Width : _fullScreenRect.Width);
			int preferredBackBufferHeight = (IsFullScreen ? _windowRect.Height : _fullScreenRect.Height);
			graphicsDeviceManager.PreferredBackBufferWidth = preferredBackBufferWidth;
			graphicsDeviceManager.PreferredBackBufferHeight = preferredBackBufferHeight;
			graphicsDeviceManager.IsFullScreen = !graphicsDeviceManager.IsFullScreen;
			graphicsDeviceManager.ApplyChanges();
			_skipChanges = false;
			EnableFullScreen(!IsFullScreen);
			Save();
			global::CutTheRope.iframework.core.Application.sharedCanvas().reshape();
			global::CutTheRope.iframework.core.Application.sharedRootController().fullscreenToggled(IsFullScreen);
		}

		public void FixWindowSize(Microsoft.Xna.Framework.Rectangle newWindowRect)
		{
			if (_skipChanges)
			{
				return;
			}
			GraphicsDeviceManager graphicsDeviceManager = Global.GraphicsDeviceManager;
			FullScreenRectChanged(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
			if (!IsFullScreen)
			{
				try
				{
					int width = graphicsDeviceManager.PreferredBackBufferWidth;
					int height = graphicsDeviceManager.PreferredBackBufferHeight;
					if (newWindowRect.Width != WindowWidth)
					{
						width = newWindowRect.Width;
					}
					if (width < MIN_WINDOW_WIDTH)
					{
						width = MIN_WINDOW_WIDTH;
					}
					if (width > MAX_WINDOW_WIDTH)
					{
						width = MAX_WINDOW_WIDTH;
					}
					if (width > ScreenWidth)
					{
						width = ScreenWidth;
					}
					if (newWindowRect.Height != WindowHeight)
					{
						height = newWindowRect.Height;
					}
					if (height < MIN_WINDOW_HEIGHT)
					{
						height = MIN_WINDOW_HEIGHT;
					}
					if (height > MAX_WINDOW_HEIGHT)
					{
						height = MAX_WINDOW_HEIGHT;
					}
					if (height > ScreenHeight)
					{
						height = ScreenHeight;
					}
					ApplyWindowSize(width, height);
				}
				catch (Exception)
				{
				}
			}
			Save();
			global::CutTheRope.iframework.core.Application.sharedCanvas().reshape();
		}

		public void ApplyViewportToDevice()
		{
			Microsoft.Xna.Framework.Rectangle bounds = ((!_isFullScreen) ? Microsoft.Xna.Framework.Rectangle.Intersect(_scaledViewRect, _windowRect) : Microsoft.Xna.Framework.Rectangle.Intersect(_scaledViewRect, _fullScreenRect));
			try
			{
				Global.GraphicsDevice.Viewport = new Viewport(bounds);
			}
			catch (Exception)
			{
			}
		}

		public void Save()
		{
			Preferences._setIntforKey(_windowRect.Width, "PREFS_WINDOW_WIDTH", false);
			Preferences._setIntforKey(_windowRect.Height, "PREFS_WINDOW_HEIGHT", false);
			Preferences._setBooleanforKey(_isFullScreen, "PREFS_WINDOW_FULLSCREEN", true);
		}

		private void WindowRectChanged(Microsoft.Xna.Framework.Rectangle newWindowRect)
		{
			if (!_skipChanges)
			{
				_windowRect = newWindowRect;
				_windowRect.X = 0;
				_windowRect.Y = 0;
				UpdateScaledView();
			}
		}

		private void FullScreenRectChanged(DisplayMode d)
		{
			FullScreenRectChanged(new Microsoft.Xna.Framework.Rectangle(0, 0, d.Width, d.Height));
		}

		private void FullScreenRectChanged(Microsoft.Xna.Framework.Rectangle r)
		{
			if (!_skipChanges)
			{
				_fullScreenRect = r;
				UpdateScaledView();
			}
		}

		private void EnableFullScreen(bool bFull)
		{
			if (!_skipChanges)
			{
				_isFullScreen = bFull;
				UpdateScaledView();
			}
		}
	}
}
