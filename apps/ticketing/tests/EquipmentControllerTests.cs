using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ticketing.Api.Contracts.Equipment;
using Ticketing.Api.Models;
using FluentAssertions;

namespace Ticketing.Api.Tests;

public class EquipmentControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public EquipmentControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEquipment_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/equipment");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateEquipment_ReturnsCreated()
    {
        var request = new CreateEquipmentRequest(
            "BOMAG BW 226", "BW226-001", EquipmentType.Roller, EquipmentManufacturer.BOMAG, "BW 226 BVC-5");

        var response = await _client.PostAsJsonAsync("/api/equipment", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var equipment = await response.Content.ReadFromJsonAsync<EquipmentResponse>(JsonOpts);
        equipment.Should().NotBeNull();
        equipment!.Name.Should().Be("BOMAG BW 226");
        equipment.SerialNumber.Should().Be("BW226-001");
    }

    [Fact]
    public async Task CreateEquipment_EmptyName_Returns400()
    {
        var request = new CreateEquipmentRequest(
            "", "SN-001", EquipmentType.Roller, EquipmentManufacturer.BOMAG, null);

        var response = await _client.PostAsJsonAsync("/api/equipment", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEquipment_DuplicateSerialNumber_Returns409()
    {
        var serial = "UNIQUE-" + Guid.NewGuid().ToString("N")[..8];
        var request1 = new CreateEquipmentRequest(
            "Equipment A", serial, EquipmentType.Roller, EquipmentManufacturer.BOMAG, null);
        var request2 = new CreateEquipmentRequest(
            "Equipment B", serial, EquipmentType.Paver, EquipmentManufacturer.CAT, null);

        var response1 = await _client.PostAsJsonAsync("/api/equipment", request1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        var response2 = await _client.PostAsJsonAsync("/api/equipment", request2);
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateEquipment_Success()
    {
        var createRequest = new CreateEquipmentRequest(
            "Before update", "UPD-" + Guid.NewGuid().ToString("N")[..8], EquipmentType.Sensor, EquipmentManufacturer.Other, null);
        var createResponse = await _client.PostAsJsonAsync("/api/equipment", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EquipmentResponse>(JsonOpts);

        var updateRequest = new UpdateEquipmentRequest(
            "After update", created!.SerialNumber, EquipmentType.Roller, EquipmentManufacturer.HAMM, "HD+ 120i");
        var updateResponse = await _client.PutAsJsonAsync($"/api/equipment/{created.Id}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<EquipmentResponse>(JsonOpts);
        updated!.Name.Should().Be("After update");
        updated.Model.Should().Be("HD+ 120i");
    }

    [Fact]
    public async Task DeleteEquipment_Success()
    {
        var createRequest = new CreateEquipmentRequest(
            "To delete", "DEL-" + Guid.NewGuid().ToString("N")[..8], EquipmentType.Software, EquipmentManufacturer.Other, null);
        var createResponse = await _client.PostAsJsonAsync("/api/equipment", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EquipmentResponse>(JsonOpts);

        var deleteResponse = await _client.DeleteAsync($"/api/equipment/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/equipment/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEquipmentById_ReturnsNotFound_WhenNotExists()
    {
        var response = await _client.GetAsync($"/api/equipment/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndGetEquipment_Roundtrip()
    {
        var serial = "RT-" + Guid.NewGuid().ToString("N")[..8];
        var createRequest = new CreateEquipmentRequest(
            "CAT CS56B", serial, EquipmentType.Roller, EquipmentManufacturer.CAT, "CS56B");
        var createResponse = await _client.PostAsJsonAsync("/api/equipment", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EquipmentResponse>(JsonOpts);

        var getResponse = await _client.GetAsync($"/api/equipment/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<EquipmentResponse>(JsonOpts);
        fetched!.Name.Should().Be("CAT CS56B");
    }
}
