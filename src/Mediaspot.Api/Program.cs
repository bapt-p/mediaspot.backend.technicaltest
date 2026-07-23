using Mediaspot.Api.DTOs;
using Mediaspot.Application.Assets.Commands.Archive;
using Mediaspot.Application.Assets.Commands.Create.CreateAudioAsset;
using Mediaspot.Application.Assets.Commands.Create.CreateVideoAsset;
using Mediaspot.Application.Assets.Commands.RegisterMediaFile;
using Mediaspot.Application.Assets.Commands.UpdateMetadata;
using Mediaspot.Application.Assets.Queries.GetById;
using Mediaspot.Application.Titles.Commands.CreateTitle;
using Mediaspot.Application.Titles.Commands.UpdateTitle;
using Mediaspot.Application.Titles.Queries.GetTitleById;
using Mediaspot.Application.Titles.Queries.GetTitles;
using Mediaspot.Infrastructure;
using Mediaspot.Infrastructure.Persistence;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure("Mediaspot.Backend.TechnicalTest");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MediaspotDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/assets/{id:guid}", async (Guid id, ISender sender) =>
    {
        var asset = await sender.Send(new GetAssetByIdQuery(id));
        return Results.Ok(asset);
    })
    .WithName("GetAssetById")
    .WithOpenApi();

app.MapPost("/assets/{id:guid}/files", async (Guid id, string path, double durationSeconds, ISender sender)
        => Results.Ok(new { mediaFileId = await sender.Send(new RegisterMediaFileCommand(id, path, durationSeconds)) }))
    .WithName("PostRegisterMediaFile")
    .WithOpenApi();

app.MapPut("/assets/{id:guid}/metadata", async (Guid id, UpdateMetadataDto dto, ISender sender) =>
    {
        await sender.Send(new UpdateMetadataCommand(id, dto.Title, dto.Description, dto.Language));
        return Results.NoContent();
    })
    .WithName("PutUpdateMetadata")
    .WithOpenApi();

app.MapPost("/assets/{id:guid}/archive", async (Guid id, ISender sender) =>
    {
        await sender.Send(new ArchiveAssetCommand(id));
        return Results.NoContent();
    })
    .WithName("PostArchiveAsset")
    .WithOpenApi();


app.MapPost("/titles", async (CreateTitleCommand cmd, ISender sender) => Results.Created("/titles", new { id = await sender.Send(cmd) }))
    .WithName("CreateTitle")
    .WithOpenApi();

app.MapPut("/titles/{id:guid}", async (Guid id, UpdateTitleDto dto, ISender sender) =>
{
    await sender.Send(new UpdateTitleCommand(id, dto.Name, dto.Description, dto.ReleaseDate, dto.Type));
    return Results.NoContent();

})
  .WithName("PutUpdateTitle")
  .WithOpenApi();

app.MapGet("/titles/{id:guid}", async (Guid id, ISender sender) =>
{
    var title = await sender.Send(new GetTitleByIdQuery(id));
    return title is null
       ? Results.NotFound()
       : Results.Ok(title);
})
    .WithName("GetTitleById")
    .WithOpenApi();

app.MapGet("/titles", async (ISender sender) =>
{
    var title = await sender.Send(new GetTitlesQuery());
    return Results.Ok(title);
})
    .WithName("GetTitle")
    .WithOpenApi();


// TYPE-SPECIFIC ASSET ENDPOINTS - SUPPORTS VIDEO AND AUDIO ASSET TYPES
// These endpoints replace the generic /assets endpoint and provide dedicated handling
// for VideoAsset (with Resolution, FrameRate, Codec) and AudioAsset (with Bitrate, SampleRate, Channels).
// Each type has its own validation, processing logic, and transcode management.

app.MapPost("/assets/video",
    async (CreateVideoAssetCommand cmd, ISender sender) =>
        Results.Created("/assets/video", new { id = await sender.Send(cmd) }))
    .WithName("PostCreateVideoAsset")
    .WithOpenApi();

app.MapPost("/assets/audio",
    async (CreateAudioAssetCommand cmd, ISender sender) =>
        Results.Created("/assets/audio", new { id = await sender.Send(cmd) }))
    .WithName("PostCreateAudioAsset")
    .WithOpenApi();



app.Run();
