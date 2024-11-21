using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ordreChange.Controllers;
using ordreChange.Services.Interfaces;
using OrdreChange.Dtos;
using System.Security.Claims;

namespace ordreChange.Tests.Unit
{

    public class OrdreControllerTests
    {
        private readonly Mock<IOrdreService> _ordreServiceMock;
        private readonly OrdreController _controller;

        public OrdreControllerTests()
        {
            _ordreServiceMock = new Mock<IOrdreService>();
            _controller = new OrdreController(_ordreServiceMock.Object);
        }
        [Fact]
        public async Task CreerOrdre_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var dto = new CreerOrdreDto { TypeTransaction = "Achat", Montant = 1000, Devise = "USD", DeviseCible = "EUR" };
            var agentId = 1;
            var response = new OrdreResponseDto { IdOrdre = 1 };

            _ordreServiceMock.Setup(s => s.CreerOrdreAsync(agentId, dto.TypeTransaction, dto.Montant, dto.Devise, dto.DeviseCible))
                             .ReturnsAsync(response);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));
                
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.CreerOrdre(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.CreerOrdre), createdAtActionResult.ActionName);
            Assert.Equal(response, createdAtActionResult.Value);
        }
        [Fact]
        public async Task GetOrdre_ReturnsOkObjectResult_WhenOrdreExists()
        {
            // Arrange
            var ordreId = 1;
            var ordreDto = new OrdreDto { IdOrdre = ordreId, TypeTransaction = "Achat", Montant = 1000, Devise = "USD", DeviseCible = "EUR" };

            _ordreServiceMock.Setup(s => s.GetOrdreDtoByIdAsync(ordreId))
                             .ReturnsAsync(ordreDto);

            // Act
            var result = await _controller.GetOrdre(ordreId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(ordreDto, okResult.Value);
        }
        [Fact]
        public async Task GetOrdre_ReturnsNotFoundResult_WhenOrdreDoesNotExist()
        {
            // Arrange
            var ordreId = 1;

            _ordreServiceMock.Setup(s => s.GetOrdreDtoByIdAsync(ordreId))
                             .ReturnsAsync((OrdreDto)null);

            // Act
            var result = await _controller.GetOrdre(ordreId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task AnnulerOrdre_ReturnsOkResult_WhenOrdreIsSuccessfullyCancelled()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "Annulé"))
                             .ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.AnnulerOrdre(ordreId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Annulation de l'ordre effectué avec succès", okResult.Value);
        }

        [Fact]
        public async Task AnnulerOrdre_ReturnsBadRequestResult_WhenOrdreCannotBeCancelled()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "Annulé"))
                             .ReturnsAsync(false);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.AnnulerOrdre(ordreId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Le statut de l'ordre ne peut pas être changé.", badRequestResult.Value);
        }

        [Fact]
        public async Task AnnulerOrdre_ReturnsForbidResult_WhenInvalidOperationExceptionIsThrown()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;
            var exceptionMessage = "Opération non autorisée";

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "Annulé"))
                             .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.AnnulerOrdre(ordreId);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task ValiderOrdre_ReturnsOkResult_WhenOrdreIsSuccessfullyValidated()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "Validé"))
                             .ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.ValiderOrdre(ordreId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Ordre validé avec succès.", okResult.Value);
        }

        [Fact]
        public async Task ValiderOrdre_ReturnsBadRequestResult_WhenOrdreCannotBeValidated()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "Validé"))
                             .ReturnsAsync(false);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.ValiderOrdre(ordreId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("L'ordre ne peut pas être validé.", badRequestResult.Value);
        }

        [Fact]
        public async Task ValiderOrdre_ReturnsForbidResult_WhenInvalidOperationExceptionIsThrown()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;
            var exceptionMessage = "Opération non autorisée";

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "Validé"))
                             .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.ValiderOrdre(ordreId);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task RefuserOrdre_ReturnsOkResult_WhenOrdreIsSuccessfullyRefused()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "A modifier"))
                             .ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.RefuserOrdre(ordreId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Refus de l'ordre effectué avec succès.", okResult.Value);
        }

        [Fact]
        public async Task RefuserOrdre_ReturnsBadRequestResult_WhenOrdreCannotBeRefused()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "A modifier"))
                             .ReturnsAsync(false);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.RefuserOrdre(ordreId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Le statut de l'ordre ne peut pas être changé.", badRequestResult.Value);
        }

        [Fact]
        public async Task RefuserOrdre_ReturnsForbidResult_WhenInvalidOperationExceptionIsThrown()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;
            var exceptionMessage = "Opération non autorisée";

            _ordreServiceMock.Setup(s => s.UpdateStatusOrdreAsync(ordreId, agentId, "A modifier"))
                             .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.RefuserOrdre(ordreId);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task ModifierOrdre_ReturnsOkResult_WhenOrdreIsSuccessfullyModified()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;
            var dto = new ModifierOrdreDto
            {
                Montant = 1000,
                Devise = "USD",
                DeviseCible = "EUR",
                TypeTransaction = "Achat"
            };

            _ordreServiceMock.Setup(s => s.ModifierOrdreAsync(ordreId, agentId, dto))
                             .ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.ModifierOrdre(ordreId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Modification de l'ordre effectuée avec succès.", okResult.Value);
        }

        [Fact]
        public async Task ModifierOrdre_ReturnsBadRequestResult_WhenOrdreCannotBeModified()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;
            var dto = new ModifierOrdreDto
            {
                Montant = 1000,
                Devise = "USD",
                DeviseCible = "EUR",
                TypeTransaction = "Achat"
            };

            _ordreServiceMock.Setup(s => s.ModifierOrdreAsync(ordreId, agentId, dto))
                             .ReturnsAsync(false);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.ModifierOrdre(ordreId, dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("L'ordre ne peut pas être modifié.", badRequestResult.Value);
        }

        [Fact]
        public async Task ModifierOrdre_ReturnsForbidResult_WhenInvalidOperationExceptionIsThrown()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;
            var dto = new ModifierOrdreDto
            {
                Montant = 1000,
                Devise = "USD",
                DeviseCible = "EUR",
                TypeTransaction = "Achat"
            };
            var exceptionMessage = "Opération non autorisée";

            _ordreServiceMock.Setup(s => s.ModifierOrdreAsync(ordreId, agentId, dto))
                             .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.ModifierOrdre(ordreId, dto);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }
        [Fact]
        public async Task GetOrdreStatutCounts_ReturnsOkResult_WithCounts()
        {
            // Arrange
            var agentId = 1;
            var counts = new { Valide = 5, Annule = 3, A_modifier = 2 };

            _ordreServiceMock.Setup(s => s.GetOrdreStatutCountsAsync(agentId))
                             .ReturnsAsync(counts);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetOrdreStatutCounts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(counts, okResult.Value);
        }
        [Fact]
        public async Task GetHistoriqueOrdre_ReturnsOkResult_WithHistoriqueDto()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;
            var historiqueDto = new HistoriqueDto { /* propriétés du DTO */ };

            _ordreServiceMock.Setup(s => s.GetHistoriqueDtoByOrdreIdAsync(agentId, ordreId))
                             .ReturnsAsync(historiqueDto);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetHistoriqueOrdre(ordreId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(historiqueDto, okResult.Value);
        }

        [Fact]
        public async Task GetHistoriqueOrdre_ReturnsNotFoundResult_WhenHistoriqueDtoIsNull()
        {
            // Arrange
            var ordreId = 1;
            var agentId = 1;

            _ordreServiceMock.Setup(s => s.GetHistoriqueDtoByOrdreIdAsync(agentId, ordreId))
                             .ReturnsAsync((HistoriqueDto)null);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetHistoriqueOrdre(ordreId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
        [Fact]
        public async Task GetOrdresByStatut_ReturnsOkResult_WithOrdreDtoList()
        {
            // Arrange
            var agentId = 1;
            var statut = "Validé";
            var ordreDtoList = new List<OrdreDto>
        {
            new OrdreDto { IdOrdre = 1, TypeTransaction = "Achat", Montant = 1000, Devise = "USD", DeviseCible = "EUR" },
            new OrdreDto { IdOrdre = 2, TypeTransaction = "Vente", Montant = 500, Devise = "EUR", DeviseCible = "USD" }
        };

            _ordreServiceMock.Setup(s => s.GetOrdreDtoByStatutAsync(agentId, statut))
                             .ReturnsAsync(ordreDtoList);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetOrdresByStatut(statut);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(ordreDtoList, okResult.Value);
        }

        [Fact]
        public async Task GetOrdresByStatut_ReturnsNotFoundResult_WhenOrdreDtoListIsEmpty()
        {
            // Arrange
            var agentId = 1;
            var statut = "Validé";
            var ordreDtoList = new List<OrdreDto>();

            _ordreServiceMock.Setup(s => s.GetOrdreDtoByStatutAsync(agentId, statut))
                             .ReturnsAsync(ordreDtoList);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, agentId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetOrdresByStatut(statut);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Aucun ordre trouvé avec le statut '{statut}'.", notFoundResult.Value);
        }
    }
}
