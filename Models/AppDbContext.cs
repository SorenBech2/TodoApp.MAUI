// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Datasync.Client.Http;
using CommunityToolkit.Datasync.Client.Offline;

using Microsoft.EntityFrameworkCore;

namespace TodoApp.MAUI.Models;

public partial class AppDbContext(DbContextOptions<AppDbContext> options) : OfflineDbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnDatasyncInitialization(DatasyncOfflineOptionsBuilder optionsBuilder)
    {
        HttpClientOptions clientOptions = new()
        {
            Endpoint = new Uri("https://app-nn4uurie34h7s.azurewebsites.net//"),
            HttpPipeline = new HttpMessageHandler[] { new LoggingHandler() }
        };
        _ = optionsBuilder.UseHttpClientOptions(clientOptions);
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        PushResult pushResult = await this.PushAsync(cancellationToken);
        if (!pushResult.IsSuccessful)
        {
            throw new ApplicationException($"Push failed: {pushResult.FailedRequests.FirstOrDefault().Value.ReasonPhrase}");
        }

        PullResult pullResult = await this.PullAsync(cancellationToken);
        if (!pullResult.IsSuccessful)
        {
            throw new ApplicationException($"Pull failed: {pullResult.FailedRequests.FirstOrDefault().Value.ReasonPhrase}");
        }
    }
}

/// <summary>
/// Use this class to initialize the database.  In this sample, we just create
/// the database. However, you may want to use migrations.
/// </summary>
/// <param name="context">The context for the database.</param>
public class DbContextInitializer(AppDbContext context) : IDbInitializer
{
    /// <inheritdoc />
    public void Initialize()
    {
        _ = context.Database.EnsureCreated();
        // Task.Run(async () => await context.SynchronizeAsync());
    }

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => context.Database.EnsureCreatedAsync(cancellationToken);
}

// Logginghandler
public class LoggingHandler : DelegatingHandler
{
    //public LoggingHandler(HttpMessageHandler innerHandler)
    //    : base(innerHandler)
    //{
    //}

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Request:");
        Console.WriteLine(request.ToString());
        if (request.Content != null)
        {
            Console.WriteLine(await request.Content.ReadAsStringAsync());
        }
        Console.WriteLine();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        Console.WriteLine("Response:");
        Console.WriteLine(response.ToString());
        if (response.Content != null)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
        Console.WriteLine();

        return response;
    }
}
