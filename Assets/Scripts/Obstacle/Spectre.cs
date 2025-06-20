using UnityEngine;

//GESTION D'UN SPECTRE 
public class Spectre : MonoBehaviour
{

    [SerializeField] float _sideMoveLatency;
    [SerializeField] AudioClip _spectreSound;
    [SerializeField] AudioClip _spectreDyingSound;
    LanesManager _LM;

    float _targetXPosition;
    Vector3 _SDVelocityRef;
    byte _currentLane;
    bool _isDying = false;
    Material _mat;
    int _spectreSoundToken;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _currentLane = _LM.GetLaneFromXPos(transform.position.x);
        _targetXPosition = transform.position.x;

        _mat = transform.Find("Body").GetComponent<SkinnedMeshRenderer>().material;
        _mat = new(_mat);
        transform.Find("Body").GetComponent<SkinnedMeshRenderer>().material = _mat;

        _spectreSoundToken = AudioManager.Instance.PlayKeepSound(_spectreSound, 1, transform.position);
    }

    // Update is called once per frame
    void Update()
    {

        if (_isDying)
        {
            Color vCol = _mat.GetColor("_BaseColor");
            vCol.a -= Time.deltaTime;
            _mat.SetColor("_BaseColor", vCol);
            if (vCol.a <= 0) Destroy(gameObject);
            return;
        }

        Vector3 vTempTargetPosition = new Vector3(_targetXPosition, transform.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
                ref _SDVelocityRef, _sideMoveLatency);

        //Lorsqu'on arrive à la nouvelle lane on choisit la prochaine aléatoirement
        if (Mathf.Abs(transform.position.x - _targetXPosition) < 0.05f)
        {
            transform.position = vTempTargetPosition;

            System.Random vRan = new();
            byte vNewLane = _currentLane;
            float? vNextLaneX = null;
            byte vloopCount = 0;
            while (vNextLaneX == null || vNewLane == _currentLane)
            {
                vloopCount++;
                if (vloopCount > 100)
                {
                    Debug.Log("Boucle infinie spectre");
                    break;
                }

                vNewLane = (byte)(_currentLane + vRan.Next(-1, 2));
                vNextLaneX = _LM.GetLaneCenter(vNewLane);
            }
            _targetXPosition = (float)vNextLaneX;
            _currentLane = vNewLane;
        }
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (_isDying) return;

        if (pOther.gameObject.CompareTag("Player"))
        {
            pOther.gameObject.GetComponent<PlayerManager>().Hurt(1);

            AudioManager.Instance.PlaySound(_spectreDyingSound, 1);
            AudioManager.Instance.StopKeepSound(_spectreSoundToken);
            _spectreSoundToken = 0;

            _isDying = true;
        }
        else if (pOther.gameObject.CompareTag("Bullet"))
        {
            pOther.gameObject.GetComponent<Projectile>().Explode();

            AudioManager.Instance.PlaySound(_spectreDyingSound, 1);
            AudioManager.Instance.StopKeepSound(_spectreSoundToken);
            _spectreSoundToken = 0;

            _isDying = true;
        }
    }

    void OnDestroy()
    {
        if (_spectreSoundToken != 0) AudioManager.Instance.StopKeepSound(_spectreSoundToken);
    }
}
