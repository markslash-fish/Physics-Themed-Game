using UnityEngine;
using Unity.Netcode;

public class PlayerSoundFX : NetworkBehaviour
{
    public AudioSource audioSource;

    [Header("Walk")]
    public AudioClip[] walkLSFX;
    public AudioClip[] walkRSFX;

    [Header("Attack")]
    public AudioClip LattackSFX;
    public AudioClip RattackSFX;
    public AudioClip heavyAttackSFX;
    public AudioClip chargeAttackSFX;

    [Header("Movement")]
    public AudioClip jumpSFX;
    public AudioClip landingSFX;
    public AudioClip dashSFX;

    // =========================
    // GENERIC PLAY
    // =========================

    void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.PlayOneShot(clip);
    }

    // =========================
    // WALK
    // =========================

    [ServerRpc]
    void WalkServerRpc(bool left)
    {
        WalkClientRpc(left);
    }

    [ClientRpc]
    void WalkClientRpc(bool left)
    {
        AudioClip[] array = left ? walkLSFX : walkRSFX;

        if (array.Length == 0) return;

        AudioClip clip = array[Random.Range(0, array.Length)];

        PlayClip(clip);
    }

    public void WalkLSFX()
    {
        if (!IsOwner) return;

        WalkServerRpc(true);
    }

    public void WalkRSFX()
    {
        if (!IsOwner) return;

        WalkServerRpc(false);
    }

    // =========================
    // LIGHT ATTACK
    // =========================

    [ServerRpc]
    void AttackServerRpc(bool left)
    {
        AttackClientRpc(left);
    }

    [ClientRpc]
    void AttackClientRpc(bool left)
    {
        PlayClip(left ? LattackSFX : RattackSFX);
    }

    public void LAttackSFX()
    {
        if (!IsOwner) return;

        AttackServerRpc(true);
    }

    public void RAttackSFX()
    {
        if (!IsOwner) return;

        AttackServerRpc(false);
    }

    // =========================
    // HEAVY
    // =========================

    [ServerRpc]
    void HeavyServerRpc()
    {
        HeavyClientRpc();
    }

    [ClientRpc]
    void HeavyClientRpc()
    {
        PlayClip(heavyAttackSFX);
    }

    public void HeavyAttackSFX()
    {
        if (!IsOwner) return;

        HeavyServerRpc();
    }

    // =========================
    // CHARGE
    // =========================

    [ServerRpc]
    void ChargeServerRpc()
    {
        ChargeClientRpc();
    }

    [ClientRpc]
    void ChargeClientRpc()
    {
        PlayClip(chargeAttackSFX);
    }

    public void ChargeAttackSFX()
    {
        if (!IsOwner) return;

        ChargeServerRpc();
    }

    // =========================
    // DASH
    // =========================

    [ServerRpc]
    void DashServerRpc()
    {
        DashClientRpc();
    }

    [ClientRpc]
    void DashClientRpc()
    {
        PlayClip(dashSFX);
    }

    public void DashSFX()
    {
        if (!IsOwner) return;

        DashServerRpc();
    }

    // =========================
    // JUMP
    // =========================

    [ServerRpc]
    void JumpServerRpc()
    {
        JumpClientRpc();
    }

    [ClientRpc]
    void JumpClientRpc()
    {
        PlayClip(jumpSFX);
    }

    public void JumpSFX()
    {
        if (!IsOwner) return;

        JumpServerRpc();
    }

    // =========================
    // LAND
    // =========================

    [ServerRpc]
    void LandServerRpc()
    {
        LandClientRpc();
    }

    [ClientRpc]
    void LandClientRpc()
    {
        PlayClip(landingSFX);
    }

    public void LandingSFX()
    {
        if (!IsOwner) return;

        LandServerRpc();
    }
}