using UnityEngine;

public interface IDamageDealer
{   
    //Calcula el daño final aplicando multiplicadores, efectos o estados propios del objeto.
    //name="baseDamage" es el daño base de la letra o elemento.
    //El daño total definitivo.
    int GetFinalDamage(int baseDamage);
}
public interface IHealer
{
    // Devuelve la cantidad de salud (en int) que restaura este objeto
    int GetHealAmount();
}

public interface IPowerUp
{
    // Devuelve un valor que multiplica el daño total de la palabra formada
    int GetPowerUpType();
}
