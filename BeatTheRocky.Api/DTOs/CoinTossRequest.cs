using System.ComponentModel.DataAnnotations;
using BeatTheRocky.Api.Core.Enums;

namespace BeatTheRocky.Api.DTOs;

public record CoinTossRequest(
    [Required] CoinChoice Choice
);
