using UnityEngine;
using System.Collections;

public class SelectAvatar : MonoBehaviour {

	public AvatarsLibraryPrefabs library;

    public GameObject testHead;
    public GameObject testBody;
    public GameObject testLegs;
    public GameObject testFeet;


    public Texture2D textureSkin;
    public Texture2D textureBody;
    public Texture2D textureLegs;
    public Texture2D textureShoes;
    public Texture2D textureHair;

    //public UMADynamicAvatar avatarLoaderPrefab;	
    public int shownModel = 0;

	private GameObject lastInstance = null;
	
	public void OnNextButton() {
		var maxModel = library.assetRecipes.Count;
		shownModel++;
		
		if (shownModel >= maxModel) {
			shownModel = 0;
		}
		
		StartCoroutine( LoadModel() );
	}
	
	public void OnPreviousButton() {
		shownModel--;
		
		if (shownModel < 0) {
			shownModel = library.assetRecipes.Count -1;
		}

        StartCoroutine( LoadModel() );
	}

	public void OnSelectButton() {
        Debug.LogError(">>>>>>>>>>>>>>>>>>>>  OnSelectButton ");

        GameObject newInstance = null;// Instantiate(prefabMale);// library.GetRecipe(shownModel));

		Player thePlayer = Player.Instance;
		if (thePlayer != null) {
			thePlayer.Avatar = newInstance;
		}

		PlayerManager playerManager = PlayerManager.Instance;
		if (playerManager != null) {
			playerManager.SelectedModel = shownModel;
		}

		RoomManager roomManager = RoomManager.Instance;
		if (roomManager != null) {
			roomManager.ToRoom ("AVATAR");
		}
	}
	
	IEnumerator LoadModel() {
		if (lastInstance != null) Destroy(lastInstance);
        yield return StartCoroutine( PlayerManager.Instance.CreateAvatar( "cabeza1", "pelo1", "torso1", "piernas1", "pies1", (instance)=>{ lastInstance = instance; } ) );
    }



    void Awake() {
	}

	void OnEnable() {
        StartCoroutine(LoadModel() );
	}
}
