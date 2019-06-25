using UnityEngine;

public class Client : MonoBehaviour
{
  public GameComponent gameComponent;

  void Start()
  {
    Game game = new Game();
    game.gameComponent = this.gameComponent;
    this.gameComponent.game = game;
    game.Start();
  }
}