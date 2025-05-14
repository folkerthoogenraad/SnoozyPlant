using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.Maui.Animations;
using SnoozyPlants.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Server;

internal class PlantImageController(PlantRepository repository) : WebApiController
{
    [Route(HttpVerbs.Get, "/image/{id}")]
    public async Task GetImage(Guid id)
    {
        // Simulate image from DB
        PlantImage? image = await repository.GetPlantImageAsync(new(id));

        if (image == null || image.Data == null)
        {
            HttpContext.Response.StatusCode = 404;
            return;
        }

        HttpContext.Response.Headers["Cache-Control"] = "public, max-age=86400"; // Cache for 1 day
        HttpContext.Response.ContentType = image.MimeType;
        await HttpContext.Response.OutputStream.WriteAsync(image.Data, 0, image.Data.Length);
    }
#if false
    public byte[] UrlEncodedBase64ToBytes(string url, out int length)
    {
        const string prefix = "data:image/png;base64,";

        if (!url.StartsWith(prefix))
        {
            throw new Exception("Failed to convert url. Not image/png base64");
        }

        ReadOnlySpan<char> base64Data = url.AsSpan().Slice(prefix.Length);

        byte[] bytes = new byte[base64Data.Length * 6 / 8];

        if(!Convert.TryFromBase64Chars(base64Data, bytes, out length))
        {
            throw new Exception("Failed to convert base 64 data to binary");
        }

        return bytes;
    }
#endif
}
