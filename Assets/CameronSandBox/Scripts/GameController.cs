using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private GameManager manager;
    private Board board;

    // Start is called before the first frame update
    void Start()
    {
        board = GameObject.Find("Grid").GetComponent<Board>();
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public List<Vector2Int> FindValidMoves(List<GameObject> attackingUnits) {
        List<Vector2Int> reachableTiles = new List<Vector2Int>();
        List<Vector2Int> validTiles = new List<Vector2Int>();

        foreach (Vector2Int cell in manager.currentPlayer.playerTiles) {
            reachableTiles.Add(cell);
            reachableTiles.AddRange(board.getNeighbors(cell));
        }
        reachableTiles = reachableTiles.Distinct().ToList();
        
        float totalAttack = CalculateUnitsAttack(attackingUnits);

        foreach (Vector2Int cell in reachableTiles) {
            if (totalAttack >= CalculateCellDefense(cell) || board[cell.x,cell.y].owner == manager.currentPlayer) validTiles.Add(cell);
        }

        return(validTiles);
    }

    public void ConquerRegion(List<GameObject> attackingUnits, Vector2Int targetRegion) {
        Player player = manager.currentPlayer;
        Vector2Int otherPlayerStart = (player.name == "player1") ? manager.player2Start : manager.player1Start;
    

        if (board[targetRegion.x,targetRegion.y].owner != null && board[targetRegion.x,targetRegion.y].owner != player) {
            List<GameObject> toResolve =  new List<GameObject>(board[targetRegion.x,targetRegion.y].GetContents());

            manager.AddScore(board[targetRegion.x,targetRegion.y].score);

            foreach(GameObject unit in toResolve) {
                unit.SetActive(false);
                if (Random.Range(0f,1f) < unit.GetComponent<Unit>().survivalRate) board.Move(unit,otherPlayerStart);
                else manager.RemoveUnit(unit);
            }
            
            foreach(GameObject unit in attackingUnits) {
                if (unit.GetComponent<Unit>().type == UnitType.Vampire) {
                    AddPiece("Thrall", targetRegion, player);
                    break;
                }
            } 
        }
        // Add score when conquering a region
        else if(board[targetRegion.x,targetRegion.y].owner != player){
            manager.AddScore(board[targetRegion.x,targetRegion.y].score);
        }


    }

    private float CalculateCellDefense(Vector2Int cell) {
        // Optional check board to see if there's anything special in that cell
        float defense = 2;
        foreach (GameObject unit in board[cell.x,cell.y].GetContents()) {
            defense += unit.GetComponent<Unit>().defense;
        }
        return(defense);
    }

    private float CalculateUnitsAttack(List<GameObject> attackingUnits) {
        float attackTotal = 0;

        foreach(GameObject movingPiece in attackingUnits) {
            attackTotal += movingPiece.GetComponent<Unit>().attack;
        }

        return (attackTotal);
    }

    public void MovePieces(List<GameObject> toMove, Vector2Int destination) {
        foreach (GameObject movingPiece in toMove) {
            board.Move(movingPiece, destination);
            if(movingPiece.GetComponent<Unit>().owner == manager.currentPlayer) {
                manager.UpdateFogOfWar(manager.currentPlayer.fogOfWar, destination);
                if (movingPiece.GetComponent<Unit>().type == UnitType.Nightwing) {
                    foreach (Vector2Int neighbor in board.getNeighbors(destination)) {
                        manager.UpdateFogOfWar(manager.currentPlayer.fogOfWar, neighbor);
                    }
                }
            }
        }
    }

    public void AddPiece(string name, Vector2Int destination, Player player) {
        GameObject newUnit = Instantiate(Resources.Load(name) as GameObject, manager.transform.position, Quaternion.identity, gameObject.transform);
        player.playerUnits.Add(newUnit);
        newUnit.GetComponent<Unit>().owner = player;
        newUnit.layer = player.name=="Player1"||player.name=="player1" ? LayerMask.NameToLayer("Player1") : LayerMask.NameToLayer("Player2");
        MovePieces(new List<GameObject>{newUnit}, destination);
    }

}
