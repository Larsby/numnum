using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using System.Collections.Generic;

[Serializable]
class PlayerData
{
	public float version;
	public int[] tableLevels;
	public int[] tableProgress;
	public int[] levelsUsed;
	public int fishWonTableIndex;
	public bool playMusic;
	public bool playSounds;
	public int bestSharkScore;
}

public class FishInfo
{
	public GameController.TableType tableType;
	public int levelIndex;

	public FishInfo (GameController.TableType tableType, int levelIndex)
	{
		this.tableType = tableType;
		this.levelIndex = levelIndex;
	}
}

public enum PropType
{
	Undefined = -1,
	Mouth = 0,
	Hat = 1,
	Musch = 2}
;

public enum SfxType
{
	Undefined = -1,
	PlayerFed = 0,
	EnemyFed = 1,
	InFront = 2,
	Behind = 3,
	Passing = 4,
	EnemyPassing = 5,
	Loses = 6,
	Wins = 7,
	Start = 8,
	EnemyStart = 9,
	IsPassed = 10,
	EnemyIsPassed = 11,
	Crunch = 12,
	WinSfx = 13,
	LoseSfx = 14,
	Cheering = 15,
	FishSelected = 16}
;

public enum SingleSfx
{
	Undefined = -1,
	AnswerCorrect = 0,
	AnswerWrong1 = 1,
	AnswerWrong2 = 2,
	Button1 = 3,
	Button2 = 4,
	ChooseFish = 5,
	PlayWithFish = 6,
	SplashUp = 7,
	SplashDown = 8,
	SplashDown2 = 9,
	Ready = 10,
	Set = 11,
	Go = 12,
	Close = 13,
	CloseLoop1 = 14,
	CloseLoop2 = 15,
	CloseLoop3 = 16}
;


public class IntermediateController : MonoBehaviour
{

	public static IntermediateController instance = null;
	private int playerFishIndex = 0;
	public int[] enabledEnemyFish;
	private PlayerData data;
	private GameController.TableType tableType;

	public int[] tableProgress;

	public Sprite[] lillaPlusTextures;
	private Sprite[] storaPlusTextures;
	private Sprite[] lillaMinusTextures;
	private Sprite[] lillaMultiTextures;
	private Sprite[] lillaDivTextures;
	private Sprite[] lillaMixTextures;
	private int[] nofFishInSet = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	// old { 7, 6, 6, 7, 6, 5 }, set in code instead

	// private int[] nofProgressStepsPerLevel_Old = { 6, 3, 5, 5, 5, 1 };
	// 6*6=36, unused, 5*5=25, 5*5=25, 5*4=20, unused	//plus, leftover, minus,mult,div,random
	int[,] nofProgressStepsPerLevel = new int[6, 7] { { 2, 3, 4, 5, 6, 6, 0 }, {0,0,0,0,0,0,0}, { 2, 2, 3, 3, 4, 5, 0 }, { 2, 3, 3, 4, 4, 6, 0 }, { 2, 2, 3, 3, 3, 0,0 }, {0,0,0,0,0,0,0} };

	private bool[] entourageEnabled = { false, false, false, false, false, false };
	private List<int> levelsUsed;
	private Dictionary<GameController.TableType, GameController.GraphicsSetType> tableToFishGraphics = new Dictionary<GameController.TableType, GameController.GraphicsSetType> ();

	private const string saveName = "/gameInfo1_1_3.dat";

	private List<FishInfo> fishCollection;
	private List<FishInfo> fishBase;
	private int fishWonTableIndex = -1;
	private bool isTextureSet = false;

	private const int nofActiveLevels = 5;
	// plus one mix level
	private const float VERSION = 1.1f;

	private AudioSource musicPlayer = null;

	private AudioSource[] sfxPlayer;
	private AudioSource[] sfxEnemy;
	private AudioSource[] singleSfxPlayer;

	private int sourceSfxPlayerCnt = 0, sourceSfxEnemyCnt = 0, singleSfxCnt = 0;
	private const int maxSources = 5;

	private AudioClip[][] sfxDatabase;
	private AudioClip[] singleSfxDatabase;
	bool levelIncreased = false;
	bool progressIncreased = false;
	int bestSharkScore = 0;


	void Awake ()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		levelsUsed = new List<int> ();

		fishCollection = new List<FishInfo> ();
		fishBase = new List<FishInfo> ();

		Load ();
					
		tableType = GameController.TableType.LillaPlus;

		DontDestroyOnLoad (gameObject);

		InitializeSounds ();
	}

	void Start ()
	{
		if (musicPlayer != null) {
			if (data.playMusic == true)
				musicPlayer.Play ();
			musicPlayer.volume = 0.15f;
			DontDestroyOnLoad (musicPlayer);
		}
	}


	// Generate arrays by, in parent folder, write:
	// echo -n { ; for f in _matad/*.wav; do echo -n \"${f:0:${#f}-4}\", ; done ; echo }\;
	// alt:
	// echo } ; for f in _matad/*.wav; do echo \"${f:0:${#f}-4}\", ; done ; echo }\;

	private void InitializeSounds ()
	{
		int i, j;

		string[][] soundClipPaths = new string[20][];
	
		soundClipPaths [0] = new string[] {
			"_matad/anahm",
			"_matad/au litta maud i maugen",
			"_matad/du den smagar fint",
			"_matad/du den var grann",
			"_matad/du va da fiddamaud",
			"_matad/epplagrod",
			"_matad/go maud",
			"_matad/grannt",
			"_matad/ha du nan kaffedoppa",
			"_matad/inga pantofflor",
			"_matad/jah lidde lomma maud",
			"_matad/jah2",
			"_matad/kauga",
			"_matad/lite fesajunken",
			"_matad/mMMm2",
			"_matad/maudabid",
			"_matad/mmmm",
			"_matad/nahm",
			"_matad/nam nam nam",
			"_matad/namnam",
			"_matad/namnamnamnam",
			"_matad/num num num num",
			"_matad/paeror",
			"_matad/rabbemos",
			"_matad/risengroed",
			"_matad/smarrit",
			"_matad/smarrit2",
			"_matad/smasken ditta",
			"_matad/spiddekauga",
			"_matad/taggar"
		};
		soundClipPaths [1] = new string[] {
			"fiskenmantavlarmot/_fi_matad/dudenvargrann",
			"fiskenmantavlarmot/_fi_matad/dudesmagarfint",
			"fiskenmantavlarmot/_fi_matad/duuuuuvardefiddamaud",
			"fiskenmantavlarmot/_fi_matad/epplagrot",
			"fiskenmantavlarmot/_fi_matad/gomaud",
			"fiskenmantavlarmot/_fi_matad/gottitdetta",
			"fiskenmantavlarmot/_fi_matad/grannt",
			"fiskenmantavlarmot/_fi_matad/haurdunankaffedoppa",
			"fiskenmantavlarmot/_fi_matad/ja1",
			"fiskenmantavlarmot/_fi_matad/jaaah",
			"fiskenmantavlarmot/_fi_matad/jaah",
			"fiskenmantavlarmot/_fi_matad/jah1",
			"fiskenmantavlarmot/_fi_matad/jah19",
			"fiskenmantavlarmot/_fi_matad/jah2",
			"fiskenmantavlarmot/_fi_matad/jah3",
			"fiskenmantavlarmot/_fi_matad/jah4",
			"fiskenmantavlarmot/_fi_matad/jah8",
			"fiskenmantavlarmot/_fi_matad/jaha2",
			"fiskenmantavlarmot/_fi_matad/jahadu",
			"fiskenmantavlarmot/_fi_matad/jahliddalommamaud",
			"fiskenmantavlarmot/_fi_matad/japp",
			"fiskenmantavlarmot/_fi_matad/kauga",
			"fiskenmantavlarmot/_fi_matad/littafesjunken",
			"fiskenmantavlarmot/_fi_matad/maudabid",
			"fiskenmantavlarmot/_fi_matad/mmm1",
			"fiskenmantavlarmot/_fi_matad/mmm2",
			"fiskenmantavlarmot/_fi_matad/mmm3",
			"fiskenmantavlarmot/_fi_matad/mums1",
			"fiskenmantavlarmot/_fi_matad/mums2",
			"fiskenmantavlarmot/_fi_matad/namnamnam4",
			"fiskenmantavlarmot/_fi_matad/numnum",
			"fiskenmantavlarmot/_fi_matad/numnum2",
			"fiskenmantavlarmot/_fi_matad/numnumnum",
			"fiskenmantavlarmot/_fi_matad/numnumnum3",
			"fiskenmantavlarmot/_fi_matad/rabbemos",
			"fiskenmantavlarmot/_fi_matad/risengrot",
			"fiskenmantavlarmot/_fi_matad/smarrigt",
			"fiskenmantavlarmot/_fi_matad/smarrit2",
			"fiskenmantavlarmot/_fi_matad/spiddekauga"
		};
		soundClipPaths [2] = new string[] {
			"_ligger_fore/auh du",
			"_ligger_fore/du du kommer bara se min rompa",
			"_ligger_fore/du ror pa pakarna",
			"_ligger_fore/du simmar som en knuda",
			"_ligger_fore/du sitt inte dar o pela",
			"_ligger_fore/du ska jag kora dig pa en rullaboer",
			"_ligger_fore/du tanker helt enavaennt",
			"_ligger_fore/har du en spaga i foden",
			"_ligger_fore/jao dar sidder du o pular",
			"_ligger_fore/odlsa inte med min ti",
			"_ligger_fore/sluta datta",
			"_ligger_fore/vao inte sao soelig",
			"_ligger_fore/var har du blitt au",
			"_ligger_fore/var inte sa dropperoeven",
			"_ligger_fore/vilket sillamjolke"
		};
		soundClipPaths [3] = new string[] {
			"fiskenmantavlarmot/_fi_ligger_fore/duauktesillakramaren",
			"fiskenmantavlarmot/_fi_ligger_fore/dubehoverhivainlittamerbranne",
			"fiskenmantavlarmot/_fi_ligger_fore/duborjarasaefterlitta",
			"fiskenmantavlarmot/_fi_ligger_fore/dudinbolebytta",
			"fiskenmantavlarmot/_fi_ligger_fore/dukasarfram",
			"fiskenmantavlarmot/_fi_ligger_fore/durorpaopokarna",
			"fiskenmantavlarmot/_fi_ligger_fore/dusimmarsomenknuda",
			"fiskenmantavlarmot/_fi_ligger_fore/dusittintedaropela",
			"fiskenmantavlarmot/_fi_ligger_fore/haurduenspaugaifoden",
			"fiskenmantavlarmot/_fi_ligger_fore/jahdarsitterduopular",
			"fiskenmantavlarmot/_fi_ligger_fore/kommerdrufraunskobygden",
			"fiskenmantavlarmot/_fi_ligger_fore/naenukanjaugintepregainmer",
			"fiskenmantavlarmot/_fi_ligger_fore/titta",
			"fiskenmantavlarmot/_fi_ligger_fore/vaurhaurdublittav",
			"fiskenmantavlarmot/_fi_ligger_fore/vikkenbobbe",
		};
		soundClipPaths [4] = new string[] {
			"_passerar/auhh",
			"_passerar/aulahomme",
			"_passerar/du e rent barhuad",
			"_passerar/du hobba daj",
			"_passerar/du nu ar jag nellad",
			"_passerar/fint vann ditta",
			"_passerar/firre",
			"_passerar/gottigt detta",
			"_passerar/haj firren",
			"_passerar/halladurd",
			"_passerar/hassleholla2",
			"_passerar/hobba",
			"_passerar/hubba di",
			"_passerar/ja den kan du sua pa",
			"_passerar/ja du kasar framm",
			"_passerar/ja nu e jao nellad",
			"_passerar/ja snaffsa mig i rumpan",
			"_passerar/jag er en fena pao detta",
			"_passerar/jah de saer vi",
			"_passerar/jah",
			"_passerar/rakt i nyllet",
			"_passerar/saer vi",
			"_passerar/sluda omma daj",
			"_passerar/sug pa den",
			"_passerar/unnan eller veck",
			"_passerar/unnan",
			"_passerar/vikken bobbe"
		};
		soundClipPaths [5] = new string[] {
			"fiskenmantavlarmot/_fi_passerar/alladom",
			"fiskenmantavlarmot/_fi_passerar/aodeeovanhuet",
			"fiskenmantavlarmot/_fi_passerar/daereenpaosporetja",
			"fiskenmantavlarmot/_fi_passerar/dufubbik",
			"fiskenmantavlarmot/_fi_passerar/duharingabyxor",
			"fiskenmantavlarmot/_fi_passerar/duhobbadig",
			"fiskenmantavlarmot/_fi_passerar/dukommerbaraseminrooommmppaaa",
			"fiskenmantavlarmot/_fi_passerar/dukommerhamnapaoboret",
			"fiskenmantavlarmot/_fi_passerar/dunuejagnellad",
			"fiskenmantavlarmot/_fi_passerar/edufrauneslov",
			"fiskenmantavlarmot/_fi_passerar/fickduvanniorat",
			"fiskenmantavlarmot/_fi_passerar/firre",
			"fiskenmantavlarmot/_fi_passerar/fjaune2",
			"fiskenmantavlarmot/_fi_passerar/fjaune3",
			"fiskenmantavlarmot/_fi_passerar/hahaha",
			"fiskenmantavlarmot/_fi_passerar/hasslaholla",
			"fiskenmantavlarmot/_fi_passerar/hejfirren",
			"fiskenmantavlarmot/_fi_passerar/hobba",
			"fiskenmantavlarmot/_fi_passerar/hobbadaj",
			"fiskenmantavlarmot/_fi_passerar/intemeddendaernej",
			"fiskenmantavlarmot/_fi_passerar/jaedesaervi",
			"fiskenmantavlarmot/_fi_passerar/jasnaffsamigirumpan",
			"fiskenmantavlarmot/_fi_passerar/rauktinyllet",
			"fiskenmantavlarmot/_fi_passerar/saervi2",
			"fiskenmantavlarmot/_fi_passerar/sarvi",
			"fiskenmantavlarmot/_fi_passerar/skrabba",
			"fiskenmantavlarmot/_fi_passerar/sludajiddra",
			"fiskenmantavlarmot/_fi_passerar/unnan",
			"fiskenmantavlarmot/_fi_passerar/unnanellerveck"
		};
		soundClipPaths [6] = new string[] {
			"fiskenmantavlarmot/_fi_vinner/degickandoganskabra",
			"fiskenmantavlarmot/_fi_vinner/enkeltsomenkausebana",
			"fiskenmantavlarmot/_fi_vinner/hemsktroligt",
			"fiskenmantavlarmot/_fi_vinner/jadenkandusuapao",
			"fiskenmantavlarmot/_fi_vinner/jadevardettaganskaroligt",
			"fiskenmantavlarmot/_fi_vinner/jaederdrogjagdigvidsnudan",
			"fiskenmantavlarmot/_fi_vinner/jagdegickjunimmt",
			"fiskenmantavlarmot/_fi_vinner/jahdegickjunummt2",
			"fiskenmantavlarmot/_fi_vinner/jahdevarredigt",
			"fiskenmantavlarmot/_fi_vinner/jaoeenfedapaudetta",
			"fiskenmantavlarmot/_fi_vinner/naehemaueda",
			"fiskenmantavlarmot/_fi_vinner/nuejagbugasprengd",
			"fiskenmantavlarmot/_fi_vinner/samlaihoppaalltienbunke",
			"fiskenmantavlarmot/_fi_vinner/skullehaftaugedyna",
			"fiskenmantavlarmot/_fi_vinner/sludadabbadig",
			"fiskenmantavlarmot/_fi_vinner/sludaommmadig",
			"fiskenmantavlarmot/_fi_vinner/sugpoudenva",
			"fiskenmantavlarmot/_fi_vinner/tabbadbaokomenvagn",
			"fiskenmantavlarmot/_fi_vinner/taggar",
			"fiskenmantavlarmot/_fi_vinner/taggartagger",
			"fiskenmantavlarmot/_fi_vinner/tappadutollan",
			"fiskenmantavlarmot/_fi_vinner/vaeintesaodropparoven",
			"fiskenmantavlarmot/_fi_vinner/vaintesaosolig",
			"fiskenmantavlarmot/_fi_vinner/vartoduvegen",
			"fiskenmantavlarmot/_fi_vinner/vilkensillamjolke"
		};
		soundClipPaths [7] = new string[] {
			"_vinner/dar dro jag di vid snudan",
			"_vinner/det gick anda ganska brao",
			"_vinner/detta de vao rolit",
			"_vinner/enkelt som en kasebana",
			"_vinner/hhemst raelit",
			"_vinner/inte mo me den",
			"_vinner/ja de gick ju nimmt",
			"_vinner/ja de var redit",
			"_vinner/ja nu e femdarsveckan slud",
			"_vinner/na nu kan jag inte prega in mer",
			"_vinner/nu e jag bugasprengd",
			"_vinner/taggar taggar",
			"_vinner/tappa du tollan",
			"_vinner/ude",
			"_vinner/var tod u vaegen"
		};
		soundClipPaths [8] = new string[] {
			"_start_av_banan_ser_fi/ah dags o spisa",
			"_start_av_banan_ser_fi/di va en ynken en",
			"_start_av_banan_ser_fi/du det var en grann badholl de har",
			"_start_av_banan_ser_fi/du va e den for en alate",
			"_start_av_banan_ser_fi/e du frann islov",
			"_start_av_banan_ser_fi/en liten pjodd",
			"_start_av_banan_ser_fi/en spedda",
			"_start_av_banan_ser_fi/har du kakat glosoppa",
			"_start_av_banan_ser_fi/ingen grann firre de",
			"_start_av_banan_ser_fi/ja dags o jobba",
			"_start_av_banan_ser_fi/ja ska skjuda fram",
			"_start_av_banan_ser_fi/jag bangar inte",
			"_start_av_banan_ser_fi/kommer du fraun skobygden",
			"_start_av_banan_ser_fi/na nu ska vi eda",
			"_start_av_banan_ser_fi/nu ble de auga av",
			"_start_av_banan_ser_fi/samla ihoppa alla i en bunke",
			"_start_av_banan_ser_fi/tidda en toppalua",
			"_start_av_banan_ser_fi/ud pa bede",
			"_start_av_banan_ser_fi/va hedder du",
			"_start_av_banan_ser_fi/va kan du hidda"
		}; 
		soundClipPaths [9] = new string[] {
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/audagospisa",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/daeredenpaohinnsirajaeret",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/devaenynkenen",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/dudevarengrannbadholldehaer",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/duvaoedeforenaulette",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/en_spedda",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/enlitenpjodd",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/entoppalua",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/fintvannditta",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/halittaimaugen",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/ingengrannfirre",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/jadagssattjobba",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/jadagssattjobba2",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/jagbangarente",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/januskajagskjutafram",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/jobbedags",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/naenuskavieda",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/nu blirdeaugaav",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/udpaobede",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/vaohederdu",
			"fiskenmantavlarmot/_fi_start_av_banan_ser_fisk/vaokanduhetta"
		};
		soundClipPaths [10] = new string[] {
			"_passerad/du auk ti sillakramaren",
			"_passerad/du din klodderov",
			"_passerad/du du kommer hamna pa boret",
			"_passerad/du flabbar bast som flabbar sist",
			"_passerad/du fubbik",
			"_passerad/du har inga bysor",
			"_passerad/du jao ska sitta falleben pa di",
			"_passerad/du kamma dig",
			"_passerad/du o ditt gapgrin",
			"_passerad/du sluta slabba sa",
			"_passerad/en illbatting den",
			"_passerad/fjaune",
			"_passerad/fjaune2",
			"_passerad/flabbar du foer",
			"_passerad/glottit",
			"_passerad/ha spillevink",
			"_passerad/haella pao o flenga runt sao daer",
			"_passerad/han e raelig den",
			"_passerad/hassleholla",
			"_passerad/hialoes",
			"_passerad/horsapara",
			"_passerad/ja du ligger inte pa soffalocket",
			"_passerad/ja tjabba kan du",
			"_passerad/jaah",
			"_passerad/jah din rabba",
			"_passerad/jahao",
			"_passerad/jajah",
			"_passerad/jobbedags",
			"_passerad/najnaj",
			"_passerad/najnajnajnaj",
			"_passerad/pass di",
			"_passerad/sluda jiddra",
			"_passerad/sluta dabba dej",
			"_passerad/sluta flabba",
			"_passerad/sluta slabba",
			"_passerad/torskaflabb",
			"_passerad/va int sao healos",
			"_passerad/va yrar du om i nattasarken"
		};
		soundClipPaths [11] = new string[] {
			"fiskenmantavlarmot/_fi_passerad/ah",
			"fiskenmantavlarmot/_fi_passerad/aulahomme",
			"fiskenmantavlarmot/_fi_passerad/baratur",
			"fiskenmantavlarmot/_fi_passerad/du",
			"fiskenmantavlarmot/_fi_passerad/dudetvartarvligtgjort",
			"fiskenmantavlarmot/_fi_passerad/duerentbarhuad",
			"fiskenmantavlarmot/_fi_passerad/dujaokommerhaffadig",
			"fiskenmantavlarmot/_fi_passerad/dukammadig",
			"fiskenmantavlarmot/_fi_passerad/duslutaslabbasao",
			"fiskenmantavlarmot/_fi_passerad/duslutaspelaallan",
			"fiskenmantavlarmot/_fi_passerad/enillbattingden",
			"fiskenmantavlarmot/_fi_passerad/flabbardufaor",
			"fiskenmantavlarmot/_fi_passerad/flabbbarbaestsomflabbarsist",
			"fiskenmantavlarmot/_fi_passerad/hardukaekatglosoppa",
			"fiskenmantavlarmot/_fi_passerad/haspillevink",
			"fiskenmantavlarmot/_fi_passerad/hassleholla2",
			"fiskenmantavlarmot/_fi_passerad/haurdusettminatollor",
			"fiskenmantavlarmot/_fi_passerad/healos",
			"fiskenmantavlarmot/_fi_passerad/horsapaera",
			"fiskenmantavlarmot/_fi_passerad/jaaaaonuedunellan",
			"fiskenmantavlarmot/_fi_passerad/jadinrabba",
			"fiskenmantavlarmot/_fi_passerad/jahh",
			"fiskenmantavlarmot/_fi_passerad/jaja",
			"fiskenmantavlarmot/_fi_passerad/koddelabbra",
			"fiskenmantavlarmot/_fi_passerad/naehe",
			"fiskenmantavlarmot/_fi_passerad/naenae",
			"fiskenmantavlarmot/_fi_passerad/naenaenae",
			"fiskenmantavlarmot/_fi_passerad/naenaenae2",
			"fiskenmantavlarmot/_fi_passerad/ofaononn",
			"fiskenmantavlarmot/_fi_passerad/paeror",
			"fiskenmantavlarmot/_fi_passerad/passadig",
			"fiskenmantavlarmot/_fi_passerad/raeligt",
			"fiskenmantavlarmot/_fi_passerad/reditraligt",
			"fiskenmantavlarmot/_fi_passerad/skajagsattafallebenpaodig",
			"fiskenmantavlarmot/_fi_passerad/sludaflabba",
			"fiskenmantavlarmot/_fi_passerad/slutadetta",
			"fiskenmantavlarmot/_fi_passerad/slutaslabba",
			"fiskenmantavlarmot/_fi_passerad/spelaallan",
			"fiskenmantavlarmot/_fi_passerad/torskaflabb",
			"fiskenmantavlarmot/_fi_passerad/vaogrinarduaud"
		};
		soundClipPaths [12] = new string[] {
			"soundeffects/bite01",
			"soundeffects/bite02",
			"soundeffects/bite03",
			"soundeffects/bite04"
		};
		soundClipPaths [13] = new string[] { "soundeffects/level_win01", "soundeffects/level_win03", "soundeffects/level_win05" };
		soundClipPaths [14] = new string[] {
			"soundeffects/level_loose02",
			"soundeffects/level_loose03",
			"soundeffects/level_loose04"
		};
		soundClipPaths [15] = new string[] {
			"soundeffects/win01",
			"soundeffects/win02",
			"soundeffects/win03",
			"soundeffects/win04",
			"soundeffects/win05",
			"soundeffects/win06",
			"soundeffects/win07",
			"soundeffects/win09"
		}; // 04-05 = cheer 6-7s
		soundClipPaths [16] = new string[] {
			"_start_av_banan_ser_fi/ja dags o jobba",
			"_start_av_banan_ser_fi/ja ska skjuda fram",
			"_start_av_banan_ser_fi/jag bangar inte",
			"_start_av_banan_ser_fi/na nu ska vi eda",
			"_start_av_banan_ser_fi/nu ble de auga av"
		}; 

		string[] singleSoundPaths = new string[] {
			"answer_correct01",
			"answer_wrong01",
			"Answer_wrong02",
			"button01",
			"button02",
			"valja_fisk",
			"valja_fisk_press_play",
			"water_splash_down01",
			"water_splash_down02",
			"water_splash_up",
			"start_klara",
			"start_fardiga",
			"start_ga",
			"getting_close01",
			"getting_close02_loop",
			"getting_close03_loop",
			"getting_close04_loop"
		};

		sfxDatabase = new AudioClip[20][];

		sfxPlayer = new AudioSource[maxSources];
		sfxEnemy = new AudioSource[maxSources];
		singleSfxPlayer = new AudioSource[maxSources];

		for (i = 0; i < maxSources; i++) {
			sfxPlayer [i] = gameObject.AddComponent<AudioSource> ();
			sfxPlayer [i].volume = 1;
			sfxEnemy [i] = gameObject.AddComponent<AudioSource> ();
			sfxEnemy [i].volume = 1;
			singleSfxPlayer [i] = gameObject.AddComponent<AudioSource> ();
			singleSfxPlayer [i].volume = 1;
		}

		for (j = 0; j < 17; j++) {
			sfxDatabase [j] = new AudioClip[soundClipPaths [j].Length];
			for (i = 0; i < soundClipPaths [j].Length; i++) {
				string fullPath = "Audio/" + soundClipPaths [j] [i];

				// I think this should only be done for iOS (if string may contain special characters, like Swedish). To be tested, if necessary.
/*				#if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
				fullPath = fullPath.Normalize (System.Text.NormalizationForm.FormD);
				#endif */

				sfxDatabase [j] [i] = (AudioClip)Resources.Load (fullPath);
				if (sfxDatabase [j] [i] == null)
					Debug.Log ("Could not load audio clip: " + soundClipPaths [j] [i] + " , index " + j + " " + i);
			}
		}

		singleSfxDatabase = new AudioClip[singleSoundPaths.Length];
		for (i = 0; i < singleSoundPaths.Length; i++) {
			string fullPath = "Audio/soundeffects/" + singleSoundPaths [i];

			singleSfxDatabase [i] = (AudioClip)Resources.Load (fullPath);
			if (singleSfxDatabase [i] == null)
				Debug.Log ("Could not load single audio clip: " + singleSoundPaths [i] + " , index " + i);
		}

	}

	public float PlayRandomFromType (SfxType sfxType, int forcedIndex = -1, float startDelay = 0)
	{
		float len = 0;

		if (!IsSoundEffectsEnabled ())
			return len;

		int sfxTypeIndex = (int)sfxType;

		int index = UnityEngine.Random.Range (0, sfxDatabase [sfxTypeIndex].Length);
		if (forcedIndex >= 0)
			index = forcedIndex;

		if (sfxType == SfxType.PlayerFed || sfxType == SfxType.InFront || sfxType == SfxType.Passing || sfxType == SfxType.Wins || sfxType == SfxType.Start || sfxType == SfxType.IsPassed) {
			sfxPlayer [sourceSfxPlayerCnt].clip = sfxDatabase [sfxTypeIndex] [index];
			len = sfxPlayer [sourceSfxPlayerCnt].clip.length;
			if (startDelay > 0)
				sfxPlayer [sourceSfxPlayerCnt].PlayDelayed (startDelay);
			else
				sfxPlayer [sourceSfxPlayerCnt].Play ();
			sfxPlayer [sourceSfxPlayerCnt].volume = 1;
			sourceSfxPlayerCnt++;
			if (sourceSfxPlayerCnt >= maxSources)
				sourceSfxPlayerCnt = 0;
			
		} else {
			sfxEnemy [sourceSfxEnemyCnt].clip = sfxDatabase [sfxTypeIndex] [index];
			len = sfxEnemy [sourceSfxEnemyCnt].clip.length;
			if (startDelay > 0)
				sfxEnemy [sourceSfxEnemyCnt].PlayDelayed (startDelay);
			else
				sfxEnemy [sourceSfxEnemyCnt].Play ();
			sfxEnemy [sourceSfxEnemyCnt].volume = 1;
			sourceSfxEnemyCnt++;
			if (sourceSfxEnemyCnt >= maxSources)
				sourceSfxEnemyCnt = 0;
		}

		return len;
	}

	public bool IsPlayingSfx (bool isPlayer)
	{
		int i;

		if (isPlayer) {
			for (i = 0; i < maxSources; i++)
				if (sfxPlayer [i].isPlaying)
					return true;
		} else {
			for (i = 0; i < maxSources; i++)
				if (sfxEnemy [i].isPlaying)
					return true;
		}
		return false;
	}

	public void StopPlayingSfx (bool isPlayer, bool stopAll = false)
	{
		int i;

		if (isPlayer || stopAll) {
			for (i = 0; i < maxSources; i++)
				if (sfxPlayer [i].isPlaying)
					sfxPlayer [i].Stop ();
		}

		if (!isPlayer || stopAll) {
			for (i = 0; i < maxSources; i++)
				if (sfxEnemy [i].isPlaying)
					sfxEnemy [i].Stop ();
		}
	}


	public float PlaySingleSfx (SingleSfx sfxIndex, bool forcePlay = false, float startDelay = 0)
	{
		float len = 0;

		if (!IsSoundEffectsEnabled () && forcePlay == false)
			return len;

		int index = (int)sfxIndex;

		singleSfxPlayer [singleSfxCnt].clip = singleSfxDatabase [index];
		len = singleSfxPlayer [singleSfxCnt].clip.length;
		if (startDelay > 0)
			singleSfxPlayer [singleSfxCnt].PlayDelayed (startDelay);
		else
			singleSfxPlayer [singleSfxCnt].Play ();
		singleSfxCnt++;
		if (singleSfxCnt >= maxSources)
			singleSfxCnt = 0;

		return len;
	}


	public void FadePlayingSfx (bool isPlayer, bool stopAll = false)
	{
		int i;

		if (isPlayer || stopAll) {
			for (i = 0; i < maxSources; i++)
				if (sfxPlayer [i].isPlaying)
					sfxPlayer [i].volume = 0.98f;
		}

		if (!isPlayer || stopAll) {
			for (i = 0; i < maxSources; i++)
				if (sfxEnemy [i].isPlaying)
					sfxEnemy [i].volume = 0.98f;
		}

		Invoke ("ContinousFade", 0.05f);
	}

	private void ContinousFade ()
	{
		bool keepFading = false;
		int i;

		for (i = 0; i < maxSources; i++)
			if (sfxPlayer [i].isPlaying && sfxPlayer [i].volume < 1) {
				keepFading = true;
				sfxPlayer [i].volume -= 0.2f;
				if (sfxPlayer [i].volume <= 0) {
					sfxPlayer [i].Stop ();
					sfxPlayer [i].volume = 1;
				}
			}

		for (i = 0; i < maxSources; i++)
			if (sfxEnemy [i].isPlaying && sfxEnemy [i].volume < 1) {
				keepFading = true;
				sfxEnemy [i].volume -= 0.2f;
				if (sfxEnemy [i].volume <= 0) {
					sfxEnemy [i].Stop ();
					sfxEnemy [i].volume = 1;
				}
			}

		if (keepFading)
			Invoke ("ContinousFade", 0.05f);
	}


	// Those annoying props positions...

	private float[] blueMouth  = { 1f,-0.25f,     1f,-0.2f,     1f,-0.28f,     1f,-0.04f,     1f,0,          1f,0.2f,       1,-0.05f,      0,0 };
	private float[] redMouth   = { 0.98f,-0.08f,  0.98f,0,      0.98f,-0.08f,  0.98f,0,       0.995f,0,      0.98f,-0.24f,  0,0,           0,0 };
	private float[] flatMouth  = { 0.99f,0,       1f,-0.08f,    0.99f, 0.04f,  0.99f,0,       0.99f,-0.16f,  0.98f,0.04f,   0,0,           0,0 };
	private float[] blowMouth  = { 0.98f,-0.08f,  0.98f,0.04f,  0.99f,0,       0.97f,0.44f,   0.98f,0.56f,   0.74f,0.68f,   0.71f,0.84f,   0,0 };
	private float[] blackMouth = { 0.99f,0,       0.99f,0.08f,  0.99f,-0.08f,  0.98f,-0.12f,  0.98f,0.04f,   0,0,           0,0,           0,0 };
	private float[] brownMouth = { 0.99f,0.09f,   0.99f,-0.2f,  0.99f,-0.12f,  0.99f,-0.2f,   0.99f,0.08f,   1f,-0.12f,     0,0,           0,0 };

	private float[] blueHat = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] redHat = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] flatHat = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] blowHat = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] blackHat = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] brownHat = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };

	private float[] blueMusch = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] redMusch = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] flatMusch = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] blowMusch = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] blackMusch = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };
	private float[] brownMusch = { 1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,  1, 0,   1, 0 };

	public float GetPropPosition (bool bX, PropType propType, int table, int levelIndex)
	{
		int ip = bX ? 0 : 1;

		if (levelIndex >= nofFishInSet [table] - 1)
			levelIndex = nofFishInSet [table] - 1;

		if (levelIndex < 0)
			levelIndex = 0;

		GameController.GraphicsSetType fishSetGfxType = tableToFishGraphics [(GameController.TableType)table];

		switch (fishSetGfxType) {
		case GameController.GraphicsSetType.Fish_Black:
			switch (propType) {
			case PropType.Mouth:
				return blackMouth [levelIndex * 2 + ip];
			case PropType.Hat:
				return blackHat [levelIndex * 2 + ip];
			case PropType.Musch:
				return blackMusch [levelIndex * 2 + ip];
			}
			break;

		case GameController.GraphicsSetType.Fish_Brown:
			switch (propType) {
			case PropType.Mouth:
				return brownMouth [levelIndex * 2 + ip];
			case PropType.Hat:
				return brownHat [levelIndex * 2 + ip];
			case PropType.Musch:
				return brownMusch [levelIndex * 2 + ip];
			}
			break;

		case GameController.GraphicsSetType.Fish_Blow:
			switch (propType) {
			case PropType.Mouth:
				return blowMouth [levelIndex * 2 + ip];
			case PropType.Hat:
				return blowHat [levelIndex * 2 + ip];
			case PropType.Musch:
				return blowMusch [levelIndex * 2 + ip];
			}
			break;

		case GameController.GraphicsSetType.Fish_Blue:
			switch (propType) {
			case PropType.Mouth:
				return blueMouth [levelIndex * 2 + ip];
			case PropType.Hat:
				return blueHat [levelIndex * 2 + ip];
			case PropType.Musch:
				return blueMusch [levelIndex * 2 + ip];
			}
			break;

		case GameController.GraphicsSetType.Fish_Red:
			switch (propType) {
			case PropType.Mouth:
				return redMouth [levelIndex * 2 + ip];
			case PropType.Hat:
				return redHat [levelIndex * 2 + ip];
			case PropType.Musch:
				return redMusch [levelIndex * 2 + ip];
			}
			break;

		case GameController.GraphicsSetType.Fish_Flat:
			switch (propType) {
			case PropType.Mouth:
				return flatMouth [levelIndex * 2 + ip];
			case PropType.Hat:
				return flatHat [levelIndex * 2 + ip];
			case PropType.Musch:
				return flatMusch [levelIndex * 2 + ip];
			}
			break;
		}

		return 0;
	}


	public void RePopulateBaseList (bool padding = true)
	{
		fishBase.Clear ();
		for (int l = 0; l < nofActiveLevels; l++) {
			int i = levelsUsed [l];
			if (i != 1) { // shark mode
				for (int j = 0; j < nofFishInSet [i]; j++) {
					fishBase.Add (new FishInfo ((GameController.TableType)i, j));
				}
				if (padding) {
					for (int j = 0; j < 7 - nofFishInSet [i]; j++) {
						fishBase.Add (new FishInfo (GameController.TableType.Undefined, 0));
					}
				}
			}
		}
	}

	public int GetLastLevelPlayed ()
	{
		return levelsUsed [0];
	}

	public void SetLastLevelPlayed (int tableIndex)
	{
//		if (levelsUsed [0] == tableIndex) // why was this needed? May have broken sth to remove, but otherwise Lilla Minus didnt work with new back button...
//			return;
		if (IsMixedTable (tableIndex))
			return;

		levelsUsed.Remove (tableIndex);
		levelsUsed.Insert (0, tableIndex);

		RePopulateBaseList ();
	}

	public void SetMusicPlayer (AudioSource musicPlayer)
	{
		if (this.musicPlayer != null)
			return;
		this.musicPlayer = musicPlayer;
	}

	public void PlayMusic ()
	{
		musicPlayer.Play ();
	}

	public void StopMusic ()
	{
		musicPlayer.Stop ();
	}

	public bool IsSoundEffectsEnabled ()
	{
		return data.playSounds;
	}

	public bool IsMusicEnabled ()
	{
		return data.playMusic;
	}

	public void SetSoundEffectsEnabled (bool state)
	{
		data.playSounds = state;
		Save ();
	}

	public void SetMusicEnabled (bool state)
	{
		data.playMusic = state;
		Save ();
	}

	public void SetTable (int newLevel)
	{
		tableType = (GameController.TableType)newLevel;
	}

	public int GetTable ()
	{
		return (int)tableType; 
	}

	public void SetPlayerFishIndex (int index)
	{
		playerFishIndex = index;
	}

	public int GetPlayerFishIndex ()
	{
		return playerFishIndex; 
	}


	public bool WasLevelIncreased() {
		return levelIncreased;
	}
	public void SetLevelIncreased(bool flag) {
		levelIncreased = flag;
	}


	public bool WasProgressIncreased() {
		return progressIncreased;
	}
	public void SetProgressIncreased(bool flag) {
		progressIncreased = flag;
	}


	public bool IncreaseFishLevel ()
	{
		levelIncreased = false;

		int tableIndex = (int)tableType;

		if (tableIndex == 1) // shark mode
			return false;

		FishInfo fishInfo = new FishInfo ((GameController.TableType)tableIndex, enabledEnemyFish [tableIndex] + 1);

		if (IsMixedTable (tableIndex)) { // for mixed table, rotate fish
			enabledEnemyFish [tableIndex]++;
			if (enabledEnemyFish [tableIndex] >= nofFishInSet [tableIndex] - 1)
				enabledEnemyFish [tableIndex] = 0;
			Save ();
			return false;
		}

		if (enabledEnemyFish [tableIndex] < nofFishInSet [tableIndex] - 1) {

			tableProgress [tableIndex]++;
			progressIncreased = true;
			if (tableProgress [tableIndex] >= nofProgressStepsPerLevel [tableIndex, enabledEnemyFish [tableIndex]]) {
				tableProgress [tableIndex] = 0;

				fishCollection.Insert (0, fishInfo);
				enabledEnemyFish [tableIndex]++;
				levelIncreased = true;
			}
		}

		if (enabledEnemyFish [tableIndex] >= nofFishInSet [tableIndex] - 1) {
			entourageEnabled [tableIndex] = true;
		}

		fishWonTableIndex = tableIndex;

		Save ();

		return levelIncreased;
	}

	public int GetFishLevel ()
	{
		return enabledEnemyFish [(int)tableType];
	}

	public void Save ()
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + saveName, FileMode.OpenOrCreate);

		for (int i = 0; i < enabledEnemyFish.Length; i++) {
			data.tableLevels [i] = enabledEnemyFish [i];
		}
		for (int i = 0; i < tableProgress.Length; i++) {
			data.tableProgress [i] = tableProgress [i];
		}
		for (int i = 0; i < levelsUsed.Count; i++) {
			data.levelsUsed [i] = levelsUsed [i];
		}
		data.version = VERSION;
		data.fishWonTableIndex = fishWonTableIndex;
		data.bestSharkScore = bestSharkScore;

		bf.Serialize (file, data);
		file.Close ();
	}

	public int Load (bool erase = false)
	{
		if (erase && File.Exists (Application.persistentDataPath + saveName)) {
			File.Delete (Application.persistentDataPath + saveName);
		}

		data = new PlayerData ();
		data.tableLevels = new int[] { 0, 0, 0, 0, 0, 0 };
//		data.tableLevels = new int[] { 6, 0, 5, 6, 4, 0 }; // all max (at the moment)
//		data.tableLevels = new int[] { 1, 0, 0, 0, 3, 0 }; // testing

		data.tableProgress = new int[] { 0, 0, 0, 0, 0, 0 };

		data.levelsUsed = new int[] { 0, 1, 2, 3, 4 };
		data.playMusic = data.playSounds = true;
		data.bestSharkScore = 0;

		if (File.Exists (Application.persistentDataPath + saveName)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + saveName, FileMode.Open);
			PlayerData savedData = (PlayerData)bf.Deserialize (file);
			for (int i = 0; i < savedData.tableLevels.Length; i++) {
				data.tableLevels [i] = savedData.tableLevels [i];
			}
			for (int i = 0; i < savedData.tableProgress.Length; i++) {
				data.tableProgress [i] = savedData.tableProgress [i];
			}
			for (int i = 0; i < savedData.levelsUsed.Length; i++) {
				data.levelsUsed [i] = savedData.levelsUsed [i];
			}
			data.version = savedData.version;
			data.playMusic = savedData.playMusic;
			data.playSounds = savedData.playSounds;
			data.bestSharkScore = savedData.bestSharkScore;
			file.Close ();
		}

		enabledEnemyFish = new int[data.tableLevels.Length];
		for (int i = 0; i < data.tableLevels.Length; i++) {
			enabledEnemyFish [i] = data.tableLevels [i];
		}

		tableProgress = new int[data.tableProgress.Length];
		for (int i = 0; i < data.tableProgress.Length; i++) {
			tableProgress [i] = data.tableProgress [i];
		}

		int nofDifferentLevelsStarted = 0;
		for (int i = 0; i < 5; i++) {
			if (i != 1 && i < 5) {
				int nofCleared = this.GetAccumulatedLevelIndex (i);
				if (nofCleared > 0)
					nofDifferentLevelsStarted++;
			}
		}

		levelsUsed.Clear ();
		for (int i = 0; i < nofActiveLevels; i++) {
			levelsUsed.Add (data.levelsUsed [i]);
		}

		bestSharkScore = data.bestSharkScore;

		// Debug.Log ("NOF_LEVELS_CLEARED: " + nofLevelsCleared);
		return nofDifferentLevelsStarted;
	}


	public bool IsFishWon (int baseIndex)
	{
		if (fishWonTableIndex == -1)
			return false;

		FishInfo info = GetFishBaseFish (baseIndex);
		if ((int)info.tableType == fishWonTableIndex && info.levelIndex == enabledEnemyFish [fishWonTableIndex])
			return true;
		return false;
	}

	public void ResetFishWonLevel ()
	{
		fishWonTableIndex = -1;
	}

	public int GetFishWonLevel ()
	{
		return fishWonTableIndex;
	}

	public void SetTextureSet (Sprite[] set, GameController.TableType type, GameController.GraphicsSetType gfxType)
	{
		switch (type) {
		case GameController.TableType.LillaPlus:
			lillaPlusTextures = set;
			break;
		case GameController.TableType.StoraPlus:
			storaPlusTextures = set;
			break;
		case GameController.TableType.LillaMinus:
			lillaMinusTextures = set;
			break;
		case GameController.TableType.LillaMulti:
			lillaMultiTextures = set;
			break;
		case GameController.TableType.LillaDiv:
			lillaDivTextures = set;
			break;
		case GameController.TableType.LillaMix:
			lillaMixTextures = set;
			break;
		default:
			break;
		}

		tableToFishGraphics.Add (type, gfxType);

		nofFishInSet [(int)type] = set.Length;
	}

	public Sprite GetEnemyFishSprite (int tableIndex = -1)
	{
		if (lillaPlusTextures.Length == 0) // uninitialized by GameController
			return null;

		if (tableIndex < 0)
			tableIndex = (int)tableType;

		int index = enabledEnemyFish [tableIndex] + 1;
		if (index >= nofFishInSet [tableIndex] - 1) {
			index = nofFishInSet [tableIndex] - 1;
		}

		switch ((GameController.TableType)tableIndex) {
		case GameController.TableType.LillaPlus:
			return lillaPlusTextures [index]; // one more than our "current" level
		case GameController.TableType.StoraPlus:
			return storaPlusTextures [index];
		case GameController.TableType.LillaMinus:
			return lillaMinusTextures [index];
		case GameController.TableType.LillaMulti:
			return lillaMultiTextures [index];
		case GameController.TableType.LillaDiv:
			return lillaDivTextures [index];
		case GameController.TableType.LillaMix:
			return lillaMixTextures [index];
		default:
			break;
		}

		return null;
	}

	public int FishCollectionSize ()
	{
		return fishCollection.Count;
	}

	public FishInfo GetFishBaseFish (int index)
	{
		if (index >= fishBase.Count)
			return null;

		return fishBase [index];
	}

	public FishInfo GetFishCollectionFish (int index)
	{
		if (index >= fishCollection.Count)
			return null;

		return fishCollection [index];
	}

	public Sprite GetSelectedFishSprite (int collectionIndex)
	{
		if (lillaPlusTextures.Length == 0) // uninitialized by GameController
			return null;

		if (collectionIndex < 0 || collectionIndex >= fishCollection.Count)
			return null;

		switch (fishCollection [collectionIndex].tableType) {
		case GameController.TableType.LillaPlus:
			return lillaPlusTextures [fishCollection [collectionIndex].levelIndex];
		case GameController.TableType.StoraPlus:
			return storaPlusTextures [fishCollection [collectionIndex].levelIndex];
		case GameController.TableType.LillaMinus:
			return lillaMinusTextures [fishCollection [collectionIndex].levelIndex];
		case GameController.TableType.LillaMulti:
			return lillaMultiTextures [fishCollection [collectionIndex].levelIndex];
		case GameController.TableType.LillaDiv:
			return lillaDivTextures [fishCollection [collectionIndex].levelIndex];
		case GameController.TableType.LillaMix:
			return lillaMixTextures [fishCollection [collectionIndex].levelIndex];
		default:
			break;
		}

		return null;
	}


	public bool HasEntourage (GameController.TableType tableType)
	{
		return (entourageEnabled [(int)tableType]);
	}

	public Sprite GetEntourageSprite (GameController.TableType tableType)
	{
		if (lillaPlusTextures.Length == 0) // uninitialized by GameController
			return null;

		switch (tableType) {
		case GameController.TableType.LillaPlus:
			return lillaPlusTextures [nofFishInSet [0] - 1];
		case GameController.TableType.StoraPlus:
			return storaPlusTextures [nofFishInSet [1] - 1];
		case GameController.TableType.LillaMinus:
			return lillaMinusTextures [nofFishInSet [2] - 1];
		case GameController.TableType.LillaMulti:
			return lillaMultiTextures [nofFishInSet [3] - 1];
		case GameController.TableType.LillaDiv:
			return lillaDivTextures [nofFishInSet [4] - 1];
		case GameController.TableType.LillaMix:
			return lillaMixTextures [nofFishInSet [5] - 1];
		default:
			break;
		}

		return null;
	}


	public int GetNofBaseFish ()
	{
		return fishBase.Count;
	}

	public Sprite GetBaseSprite (int baseIndex)
	{
		if (baseIndex >= fishBase.Count)
			return null;

		FishInfo fishInfo = fishBase [baseIndex];

		GameController.TableType tableType = fishInfo.tableType;
		if (tableType == GameController.TableType.Undefined)
			return null;
		int index = fishInfo.levelIndex;

		if (fishInfo.levelIndex >= nofFishInSet [(int)tableType])
			return null;

		switch (tableType) {
		case GameController.TableType.LillaPlus:
			return lillaPlusTextures [index];
		case GameController.TableType.StoraPlus:
			return storaPlusTextures [index];
		case GameController.TableType.LillaMinus:
			return lillaMinusTextures [index];
		case GameController.TableType.LillaMulti:
			return lillaMultiTextures [index];
		case GameController.TableType.LillaDiv:
			return lillaDivTextures [index];
		case GameController.TableType.LillaMix:
			return lillaMixTextures [index];
		default:
			break;
		}
		return null;
	}

	public int GetFishInCollectionIndex (int baseIndex)
	{
		for (int i = 0; i < fishCollection.Count; i++) {
			if (fishCollection [i].levelIndex == fishBase [baseIndex].levelIndex && fishCollection [i].tableType == fishBase [baseIndex].tableType) {
				return i;
			}
		}
		return -1;
	}

	public int GetNofFishInSet (int index)
	{
		return nofFishInSet [index];
	}

	public bool IsMixedTable (int index)
	{
		return index == nofActiveLevels || index == 1;
	}


	public int GetTableProgress (int index)
	{
		return tableProgress[index];
	}

	public float GetRelativeTableProgress (int index, bool bPreviousProgress = false)
	{
		float progress = tableProgress [index];
		if (bPreviousProgress)
			progress--;

		float retVal = progress / (float)nofProgressStepsPerLevel[index, enabledEnemyFish [index]];
		return Mathf.Clamp01(retVal);
	}


	public int GetNofUnmixedTables ()
	{
		return nofActiveLevels;
	}

	public int GetTableLevel (int tableIndex)
	{
		if (tableIndex < 0 || tableIndex >= nofActiveLevels)
			return 0;
		return enabledEnemyFish [tableIndex];
	}


	public void SetTableLevel (int tableIndex, int level, bool bResetProgress = true)
	{
		if (tableIndex < 0 || tableIndex >= nofActiveLevels)
			return;
		enabledEnemyFish [tableIndex] = level;
		if (bResetProgress)
			tableProgress [tableIndex] = 0;
	}


	public bool IsTexturesSet ()
	{
		return isTextureSet;
	}

	public void SetTexturesFinished ()
	{
		isTextureSet = true;
		fishCollection.Clear ();

		for (int i = 0; i < nofActiveLevels; i++) {
			for (int j = 0; j < nofFishInSet [i]; j++) {
				if (j <= enabledEnemyFish [i])
					fishCollection.Add (new FishInfo ((GameController.TableType)i, j));
			}
			if (enabledEnemyFish [i] >= nofFishInSet [i] - 1)
				entourageEnabled [i] = true;
			else
				entourageEnabled [i] = false;
		}

		RePopulateBaseList ();
	}

	public Color IntColor(float r, float g, float b, float a = 255) {
		return new Color (r/255f, g/255f, b/255f, a/255f);
	}


	public int GetAccumulatedLevelIndex(int tableIndex) {
		// int index = enabledEnemyFish [tableIndex] * nofProgressStepsPerLevel[tableIndex];

		int index = 0;

		for (int i = 0; i < enabledEnemyFish [tableIndex]; i++)
			index += nofProgressStepsPerLevel[tableIndex, i];

		index += tableProgress[tableIndex];

		//Debug.Log ("GALI: " + index);

		return index;
	}


	public int GetAccumulatedMaxLevel(int tableIndex) {
		//	return (nofFishInSet[tableIndex] - 1) * nofProgressStepsPerLevel [tableIndex] - 1;
		int index = 0;

		for (int i = 0; i < nofFishInSet[tableIndex]; i++)
			index += nofProgressStepsPerLevel[tableIndex, i];

		//Debug.Log ("GAML: " + index);

		return index;
	}

	public int GetMaxLevel(int tableIndex) {
		return (nofFishInSet [tableIndex] - 1);
	}

	public int GetBestSharkScore() {
		return bestSharkScore;
	}

	public void SetBestSharkScore(int score) {
		bestSharkScore = score;
	}

}
