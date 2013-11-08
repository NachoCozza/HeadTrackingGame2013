using UnityEngine;
using System.Collections;

public class Arma : MonoBehaviour {
	
	
	//atributos
	protected int bullets;
	public enum WeaponEnum {Laser = 0, Rifle = 1};
	public WeaponEnum weaponModel = 0;
	public int damage;
	public int capacity;
	public float laserSpeed;
	public bool shooting = false;
	float muzzleDeltaTime = 0;
	public float overheatCount;
	private int overheatLimit=25;
	
	public float fireRate;
	private float timer = 0f;
	private float timeroverHeat=0;
	
	//Dependencias
	public Transform spawnPos;
	public Rigidbody proPrefab;
	private PlayerBehave player;
	
	GUIText ammoTxt;
	GUIText warningTxt;

	public GameObject muzzle;
	public GameObject handgunAudio, laserAudio, nobulletAudio;
	public Material laserMaterial, materialInstance;
	private Color color1;
	public float redIncrRate;

	
	public void shoot(){
		
		//falta checkear que el jugador se quedo sin balas
				
		if (timer >= fireRate) {
		
			if (this.bullets<=0){
				showWarning("reload");
				if (weaponModel == WeaponEnum.Rifle){
					nobulletAudio.audio.Play ();
				}
			}
			else {
				
				
				//shooting = true;  SAQUE ESTE SHOOTING = TRUE Y HICE QUE REALMENTE SOLO CUANDO ESTE DISPARANDO, CONSULTAR A COZZA SI ESTA BIEN EN CLASE.
				
				if (weaponModel == WeaponEnum.Laser && overheatCount<overheatLimit) 				
				{
					Rigidbody lasInstance = (Rigidbody) Instantiate(proPrefab, spawnPos.position, spawnPos.rotation);
					lasInstance.AddForce(spawnPos.forward*laserSpeed);	
					lasInstance.GetComponent<Projectile>().setDamage(damage);
					laserAudio.audio.Play ();
					overHeat();	
					shooting = true;
					WiiMote.wiimote_rumble(WiiMote.pointerWiimote, (float)0.073);
				}			
				else if (overheatCount>overheatLimit){
					WiiMote.wiimote_rumble(WiiMote.pointerWiimote, (float)2);
				}
				else if (weaponModel == WeaponEnum.Rifle)
				{
					handgunAudio.audio.Play ();
					Vector3 fwd = spawnPos.TransformDirection(Vector3.forward);
					fwd *= 20;
					WiiMote.wiimote_rumble(WiiMote.pointerWiimote, (float)0.2);
					Debug.DrawRay(transform.position,fwd);
					
					RaycastHit hit;
					if (Physics.Raycast(transform.position, fwd, out hit, 9999, 1))
					{
						//para cuando queres levantar el arma2
						if(Round.weaponSpawned && !Round.weaponPicked) {
							if (hit.collider.GetComponent<Arma>() != null) {
								hit.collider.GetComponent<WeaponSpawn>().transitionToPlayer();
							}
						}
						GameObject particleInstance = (GameObject) Instantiate(Projectile.bulletParticle, hit.point, transform.rotation);
						particleInstance.transform.LookAt(transform.position);
						Destroy(particleInstance, particleInstance.particleSystem.duration + 0.01f);
						HealthSystem HP = hit.collider.gameObject.GetComponent<HealthSystem>();
						
						if (HP != null){
							HP.damageHp(damage);
						}
					}
					shooting = true;
				}				
				else{
					shooting = false;
				}
				
				
				this.bullets-=1;
				updateAmmo();
				
				if (this.bullets==0) {
					showWarning("reload");	
				}
				
			}
			timer = 0f;
		}
	}
	
	public void overHeat() {
		overheatCount+=1;	
		Color newCol = renderer.material.color;
		newCol.r += redIncrRate;
		renderer.material.color = newCol;
			
	}
	
	public void decreaseOverHeat(){
		if (shooting == false && overheatCount>0){
			timeroverHeat+= Time.deltaTime;
			if (timeroverHeat>1.5f){
				overheatCount-=Time.fixedDeltaTime*4;
				Color newCol = renderer.material.color;
				newCol.r -= redIncrRate * Time.fixedDeltaTime*4;
				renderer.material.color = newCol;
				
			}
		}
		else{
			timeroverHeat=0;
		}
	}
	
	private void updateAmmo() {
		ammoTxt.text = this.bullets + "/" + this.capacity;
	}
	
	private void showWarning (string msg) {
		switch (msg.ToLower()) {
		case "reload":
			warningTxt.enabled = true;
			warningTxt.text = "Reload!";
			break;
			
		case "hide":
			warningTxt.enabled = false;
			break;
			
		case "empty":
			warningTxt.enabled = true;
			warningTxt.text = "out of ammo";
			break;
			
			
		}
	}
	
	
	
	
	
	public int reload (int totalBalas) {
	
		if (totalBalas==0){
			showWarning("empty");
		}
		
		else if (totalBalas>this.capacity || (totalBalas+this.bullets)>this.capacity){
			int aux = this.capacity - this.bullets;
			this.bullets = capacity;
			showWarning("hide");
			updateAmmo();
			return aux;
		}
		
		else{
			this.bullets+= totalBalas;	
			updateAmmo();
			showWarning("hide");
		}
		
		return totalBalas;//devuelve siempre totalBlas salvo el 2do caso. El return es las balas que va a restar
	}	
	
	
	void Start () {	
		this.bullets = this.capacity;
		player = Enemy.player.GetComponent<PlayerBehave>();
		ammoTxt = player.ammoGUI;
		warningTxt = player.warningGUI;
		updateAmmo();
		muzzle.SetActive(false);
		redIncrRate = 5f/overheatLimit;
		//color1 = renderer.material.color;
	}
	
	void Update () {
		
		if (player.weapon == this) {
			gameObject.transform.LookAt(Crosshair.getWiimoteCrosshair());
		}
		
		timer += Time.deltaTime;
		decreaseOverHeat();
		
		if (shooting == true) {
			muzzle.SetActive(true);
			muzzleDeltaTime += Time.deltaTime;
			if (muzzleDeltaTime > 0.15) {
				muzzleDeltaTime = 0;
				muzzle.transform.Rotate(new Vector3(0,0, Random.Range(-90,90)));
				shooting= false;
				muzzle.SetActive(false);
				
			}
		}
		
	}

}
