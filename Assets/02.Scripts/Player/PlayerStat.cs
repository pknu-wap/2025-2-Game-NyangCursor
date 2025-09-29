using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    // �Ϲݸ�� �迭
    [Header("�̵��迭")]
    public float walkSpeed;
    public float attackDamage;


    //���� �迭
    [Header("����迭")]
    [Tooltip("���� �ڿ����� �ӵ� (�ʴ�). 20�� Ǯ�����̸� 5")]
    public float fuelRechargePerSec = 5f;
    [Tooltip("���� �Ҹ� �ӵ� (�ʴ�)")]
    public float fuelConsumePerSec = 10f;


    // ��������̺� �迭
    [Header("Overdrive�迭")]
    public float speed = 6f; //�⺻�� 6
    [Range(1, 10)] public float turnRateDeg = 3f; //(1= ��������, 3 = ���۰�, 10 = ����)
    [Tooltip("���� 1 �Ҹ�� ��������̺� ������ (�⺻ 0.7)")]
    public float odGainPerFuel = 0.7f;
    [Tooltip("Ű�� �� �� ���� ���۱��� ����(��)")]
    public float odDecayDelay = 3f;
    [Tooltip("���� ���� �� �ʴ� ���ҷ�")]
    public float odDecayPerSec = 8f;
    [Tooltip("���� �ν�Ʈ ������Ʈ�� ���� ���� �ð�(��)")]
    public float boostOnDuration = 1f;
    [Tooltip("���� �ν�Ʈ �߰� ���ӷ�")]
    public float boostExtraSpeed = 1.3f;


    // ���� �迭
    public int maxHP = 3;
    public float shield = 0f;

    // ��Ÿ �迭
    public float goldMultiplier = 1f; //���ȹ�淮
    public float expMultiplier = 1f;  //����ġȹ�淮

    // ���߿� Ȯ�� ����
}
