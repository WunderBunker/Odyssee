using UnityEngine;

//GESTION DU BATEAU SERVANT DE SHOWCASE DANS LE SHOP
public class BoatShowcase : MonoBehaviour
{
    [SerializeField] float _rotationSpeed = 1;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, _rotationSpeed * Time.deltaTime));
    }
}
