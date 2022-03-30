using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MoveSelector : MonoBehaviour
{
    private int unitsMask;
    private int p1Mask;
    private int p2Mask;

    private GameObject tileHighlight;
    private List<GameObject> movingPieces = new List<GameObject>();
    private Board board;

    private GameManager manager;

    public Text toggle;

    private bool isActive = false;

    private List<Vector2Int> possibleMoves = new List<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        this.enabled = false;
        p1Mask  = LayerMask.GetMask("Player1");
        p2Mask  = LayerMask.GetMask("Player2");
        manager = GameManager.instance;
        board = GameObject.Find("Grid").GetComponent<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Function When you press left click
    void OnSelect()
    {
        if(isActive) {
            // not sure why needed but will throw an error otherwise
            if(this.enabled){
                Vector2 ray = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                
                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
                if(hit)
                {
                    Vector3 point = hit.point;

                    Vector3Int currentCell = manager.map.WorldToCell(point);
                    Vector2Int boardCell = board.CoordsTilemapToBoard(currentCell);

                    int attackTotal = 0;
                    foreach(GameObject movingPiece in movingPieces) {
                        attackTotal += movingPiece.GetComponent<Unit>().attack;
                    }

                    Debug.Log(board[boardCell.x,boardCell.y].owner == manager.currentPlayer.name);
                    Debug.Log(attackTotal >= board[boardCell.x,boardCell.y].cost);
                    bool canConquer = attackTotal >= board[boardCell.x,boardCell.y].cost || board[boardCell.x,boardCell.y].owner == manager.currentPlayer.name;

                    if(possibleMoves.Contains(boardCell) && canConquer)
                    {
                        board.ConquerRegion(boardCell);
                        foreach (GameObject movingPiece in movingPieces) {
                            board.Move(movingPiece, board.CoordsTilemapToBoard(currentCell));
                        }
                        ExitState();
                        StartCoroutine(manager.EnableTurnSplash());
                    }
                    else {
                        Debug.Log("invalid move");
                    }
                }
            }
            else {
                Vector2 ray = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                unitsMask = manager.currentPlayer.name == "player1" ? p1Mask : p2Mask;

                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, distance: Mathf.Infinity, layerMask: unitsMask);

                if(hit) {
                    GameObject isHit = hit.collider.gameObject;
                    if(isHit.tag == "Unit") 
                    {
                        SelectUnit(isHit);

                        Vector3Int currentCell = manager.map.WorldToCell(isHit.transform.position);
                        Vector2Int boardCell = board.CoordsTilemapToBoard(currentCell);

                        foreach (Vector2Int p in board.getNeighbors(boardCell))
                        {
                            possibleMoves.Add(p);
                        }
                    }
                }
            }
        }  
    }

    void OnHitEnter()
    {
        if(isActive) {
            if(!this.enabled) EnterState();
            else ExitState();
        }
    }

    private void SelectUnit(GameObject unit) {
        if (movingPieces.Contains(unit)) {
            movingPieces.Remove(unit);
            unit.SendMessage("Deselect");
        }
        else {
            movingPieces.Add(unit);
            unit.SendMessage("Select");
        }
    }

    public void Freeze() {
        isActive = false;
        toggle.text = "";
    }

    public void Unfreeze() {
        isActive = true;
        toggle.text = "Selecting Units";
    }

    private void EnterState()
    {
        this.enabled = true;
        toggle.text = "Selecting Destination";
    }

    private void EndState(Player winner)
    {
        this.enabled = false;
        toggle.text = "The winner is" + winner.name;
    }
    
    private void ExitState()
    {
        this.enabled = false;
        foreach (GameObject movingPiece in movingPieces) {
            movingPiece.GetComponent<Unit>().Deselect();
        }
        movingPieces = new List<GameObject>();
        toggle.text = "Selecting Units";
        possibleMoves = new List<Vector2Int>();
    }
}
