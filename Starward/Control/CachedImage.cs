﻿using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Starward.Service;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Starward.Control;

public sealed class CachedImage : ImageEx
{


    private static readonly ConcurrentDictionary<Uri, Uri> fileCache = new();


    protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        //if (!CacheService.Instance.Initialized)
        //{
        //    var folder = Path.Join(AppConfig.ConfigDirectory, "cache");
        //    Directory.CreateDirectory(folder);
        //    CacheService.Instance.Initialize(await StorageFolder.GetFolderFromPathAsync(folder));
        //}
        try
        {
            if (imageUri.Scheme is "ms-appx" or "file")
            {
                return new BitmapImage(imageUri);
            }
            else
            {
                if (fileCache.TryGetValue(imageUri, out var uri))
                {
                    return new BitmapImage(uri);
                }
                else
                {

                    var file = await CacheService.Instance.GetFromCacheAsync(imageUri, false, token);
                    if (token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException("Image source has changed.");
                    }
                    if (file is null)
                    {
                        throw new FileNotFoundException(imageUri.ToString());
                    }
                    uri = new Uri(file.Path);
                    fileCache[imageUri] = uri;
                    return new BitmapImage(uri);
                }
            }
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception)
        {
            await CacheService.Instance.RemoveAsync(new[] { imageUri });
            throw;
        }
    }
}