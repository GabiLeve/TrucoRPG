using Moq;
using Xunit;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TrucoRPG.API.Hubs;
using TrucoRPG.API.Services; 

namespace TrucoRPG.Tests;

public class GameHubTests
{
    // 1. Declaramos los campos que usaremos en todos los tests
    private readonly Mock<SalaService> _mockSalas;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly GameHub _hub;

    public GameHubTests()
    {
        
    }

   
}
