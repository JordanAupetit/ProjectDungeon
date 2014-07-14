using UnityEngine;
using System.Collections;
using System.Text;
using System.IO; 
//using Pathfinding;
using UnityEditor;

public class Generate_Dungeon : MonoBehaviour {

	private string[,] map;
	public GameObject wall_prefab;
	public GameObject tinywall_prefab;
	public GameObject ground_prefab;
	public GameObject base_prefab;
	public int max_colonnes = 400;
	public int max_lignes = 400;
	public bool enable = true;

	public GameObject friendlyPrefab;
	public GameObject enemyPrefab;

	void Awake () {
		if (!enable)
			return;
			
		Debug.Log ("Dungeon creation started!");

		map = new string[max_colonnes,max_lignes];

		for (int col = 0; col < max_colonnes; col++) {
			for (int line = 0; line < max_lignes; line++) {
				map[col,line] = "-";
			}
		}

		file_to_map ("Ressources/map1.txt", ref map);
		Debug.Log ("File to map => OK");
		

		for (int col = 0; col < max_colonnes; col++) {
			for (int line = 0; line < max_lignes; line++) {
				instantiate_gameobject(map[col,line], col, line);
			}
		}

		Debug.Log ("Generated");
		//AstarPath.active.Scan();
		NavMeshBuilder.BuildNavMesh();
		Debug.Log ("Scanned");
	}

	void instantiate_gameobject(string symbol, int col, int line){

		GameObject obj;

		switch (symbol) 
		{
			case "1":	// Wall
				obj = Instantiate (wall_prefab, new Vector3(0 + (col * 2), 2, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.layer = 8;
				break;
			case "2":	// Spawn sbire place
				obj = Instantiate (ground_prefab, new Vector3(0 + (col * 2), 0, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.layer = 9;
				obj = Instantiate (base_prefab, new Vector3(0 + (col * 2), 1.1f, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.tag = "FriendlyBaseTAG";

				Spawner spF;
				spF = obj.AddComponent<Spawner>();
				spF.numberOfAgents = 40;
				spF.unitPrefab = friendlyPrefab;
				break;
			case "3":	// Empty
				break;
			case "4":	// Spawn heros place
				obj = Instantiate (ground_prefab, new Vector3(0 + (col * 2), 0, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.layer = 9;
				obj = Instantiate (base_prefab, new Vector3(0 + (col * 2), 1.1f, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.tag = "EnemyBaseTAG";

				//Spawner spE;
				//spE = obj.AddComponent<Spawner>();
				//spE.numberOfAgents = 20;
				//spE.unitPrefab = enemyPrefab;
				break;
			case "5":	// Rooms (node)
				obj = Instantiate (ground_prefab, new Vector3(0 + (col * 2), 0, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.layer = 9;
				obj = Instantiate (base_prefab, new Vector3(0 + (col * 2), 0.1f, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.tag = "RoomTAG";
				break;
			case "6":
				break;
			case "7":	// Chests
				obj = Instantiate (ground_prefab, new Vector3(0 + (col * 2), 0, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.layer = 9;
				obj = GameObject.Instantiate (base_prefab, new Vector3(0 + (col * 2), 0.1f, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.tag = "ChestBaseTAG";
				break;
			case "8":	// Sol
				obj = Instantiate (ground_prefab, new Vector3(0 + (col * 2), 0, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.layer = 9;
				break;
			case "9":
				break;
			case "10":
				break;
			case "11":	// Tiny Wall
				obj = Instantiate (tinywall_prefab, new Vector3(0 + (col * 2), 1, 0 - (line * 2)), Quaternion.identity) as GameObject;
				obj.layer = 8;
				break;
			case "12":
				break;
			default:
				break;
		}
	}

	void file_to_map(string fileName, ref string[,] map){

		string line;
		string[] line_split;
		int index_colonne = 0;
		int index_ligne = 0;
		int line_to_remove = 12;
		int cpt = 0;
		// Create a new StreamReader, tell it which file to read and what encoding the file
		// was saved as
		StreamReader myFile = new StreamReader(Application.dataPath + "/" + fileName, Encoding.Default);

		using (myFile)
		{
			// While there's lines left in the text file, do this:
			do
			{
				line = myFile.ReadLine();

				if ((cpt >= line_to_remove) && line != null) {

					line_split = line.Split(',');
					for(index_colonne = 0; index_colonne < line_split.Length; index_colonne++){
						//Debug.Log ("Ligne " + index_ligne + " Colonne " + index_colonne + " taille ligne " + line_split.Length);
						map[index_colonne,index_ligne] = line_split[index_colonne].ToString(); 
					}
					index_ligne++;
				}
				cpt++;
			}
			while (line != null);
			
			// Done reading, close the reader and return true to broadcast success    
			myFile.Close();
		}
		
	}
}
