using BeatTheRocky.Api.Core.Models;

namespace BeatTheRocky.Api.Factories;

public interface IGameFactory
{
    GameSession CreateNewSession();
}
