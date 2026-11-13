using System.ComponentModel.DataAnnotations;

namespace BeatTheRocky.Api.DTOs;

public record MakeMoveRequest(
    [Range(0, 8, ErrorMessage = "Cell index must be between 0 and 8.")] int CellIndex
);
