using UnityEngine;

[RequireComponent(typeof(PlayerEntity))]
[RequireComponent(typeof(PlayerLocomotionBrain))]
public class PlayerStaminaSystem : MonoBehaviour
{
    [SerializeField] private float sprintConsumePerSecond = 4f;
    [SerializeField] private float walkRecoverPerSecond = 3f;
    [SerializeField] private float idleRecoverPerSecond = 5f;
    [SerializeField] private float resumeSprintThreshold = 0.3f;
    private float staminaAccumulator;
    private bool staminaInitialized = false;


    private PlayerEntity entity;
    private PlayerLocomotionBrain locomotionBrain;

    private bool canSprint = true;
    private bool hasLoggedWaitingData = false;

    public bool CanSprint() => canSprint;

    private void Awake()
    {
        locomotionBrain = GetComponent<PlayerLocomotionBrain>();
    }

    private void Update()
    {
        entity = PlayerLocator.Instance.GetPlayerEntity();

        if (entity == null || locomotionBrain == null)
            return;

        if (entity.Data == null || entity.Data.attributeData == null || entity.Data.runtimeData == null)
        {
            if (!hasLoggedWaitingData)
            {
                Debug.Log("[PlayerStaminaSystem] waiting player data init...");
                hasLoggedWaitingData = true;
            }
            return;
        }

        if (hasLoggedWaitingData)
        {
            Debug.Log("[PlayerStaminaSystem] player data ready.");
            hasLoggedWaitingData = false;
        }

        PlayerAttributeData attr = entity.Data.attributeData;
        PlayerRuntimeData runtime = entity.Data.runtimeData;

        int maxStamina = Mathf.Max(0, attr.maxStamina);
        if (maxStamina <= 0)
        {
            if (runtime.currentStamina != 0)
            {
                runtime.currentStamina = 0;
                EventBus.Publish(new PlayerStaminaChangedEvent(runtime.currentStamina, maxStamina));
            }

            canSprint = false;
            locomotionBrain.SetSprintEnabled(false);
            staminaInitialized = false;
            return;
        }

        if (!staminaInitialized)
        {
            staminaAccumulator = runtime.currentStamina;
            staminaInitialized = true;
        }

        Vector2 moveInput = locomotionBrain.CurrentCommand.move;
        bool wantsSprint = locomotionBrain.CurrentCommand.sprint;
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isActuallySprinting = isMoving && wantsSprint && canSprint;

        float delta;
        if (isActuallySprinting)
        {
            delta = -sprintConsumePerSecond * Time.deltaTime;
        }
        else if (isMoving)
        {
            delta = walkRecoverPerSecond * Time.deltaTime;
        }
        else
        {
            delta = idleRecoverPerSecond * Time.deltaTime;
        }

        int oldStamina = runtime.currentStamina;

        staminaAccumulator += delta;
        staminaAccumulator = Mathf.Clamp(staminaAccumulator, 0f, maxStamina);

        runtime.currentStamina = Mathf.RoundToInt(staminaAccumulator);

        if (runtime.currentStamina <= 0)
        {
            canSprint = false;
        }

        if (!canSprint && runtime.currentStamina >= Mathf.CeilToInt(maxStamina * resumeSprintThreshold))
        {
            canSprint = true;
        }

        locomotionBrain.SetSprintEnabled(canSprint);

        if (runtime.currentStamina != oldStamina)
        {
            EventBus.Publish(new PlayerStaminaChangedEvent(runtime.currentStamina, maxStamina));
        }
    }

}
