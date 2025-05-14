using EmbedIO;
using EmbedIO.WebApi;
using SnoozyPlants.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Server;

public class PlantImageServer
{
    private readonly string baseUrl = "http://localhost:8219";
    private WebServer _server;

    public PlantImageServer(PlantRepository repository)
    {
        _server = new WebServer(o => o
                .WithUrlPrefix(baseUrl)
                .WithMode(HttpListenerMode.Microsoft))
            .WithCors("*", "*", "*")
            .WithLocalSessionManager()
            .WithWebApi("/", m => m.WithController(() => new PlantImageController(repository)));

        _server.RunAsync();
    }

    public string CreateImageUrl(PlantId id, Guid cacheBustVersion)
    {
        return string.Format("{0}/image/{1}?bust={2}", baseUrl, id, cacheBustVersion);
    }
}
