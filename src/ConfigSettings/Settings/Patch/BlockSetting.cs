﻿namespace ConfigSettings.Patch
{
  /// <summary>
  /// Настройки блока.
  /// </summary>
  internal class BlockSetting
  {
    /// <summary>
    /// Доступность блока.
    /// </summary>
    public bool? IsEnabled { get; }

    /// <summary>
    /// Содержимое блока.
    /// </summary>
    public string Content { get; }
    
    /// <summary>
    /// Содержимое блока без корневого заголовка.
    /// </summary>
    public string ContentWithoutRoot { get; }

    public string FilePath { get; }

    public BlockSetting(bool? isEnabled, string content, string contentWithoutRoot, string filePath)
    {
      this.IsEnabled = isEnabled;
      this.Content = content;
      this.ContentWithoutRoot = contentWithoutRoot;
      this.FilePath = filePath;
    }
  }
}
