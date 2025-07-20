using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using TMPro;
using LazyFollow = UnityEngine.XR.Interaction.Toolkit.UI.LazyFollow;
using System.Diagnostics;
using System.Linq;

namespace UnityEngine.XR.Templates.MR
{
    public struct Goal
    {
        public GoalManager.OnboardingGoals CurrentGoal;
        public bool Completed;

        public Goal(GoalManager.OnboardingGoals goal)
        {
            CurrentGoal = goal;
            Completed = false;
        }
    }

    public class GoalManager : MonoBehaviour
    {
        public enum OnboardingGoals
        {
            Empty,
            FindSurfaces,
            TapSurface,
        }

        Queue<Goal> m_OnboardingGoals;
        Goal m_CurrentGoal;
        bool m_AllGoalsFinished;
        int m_SurfacesTapped;
        int m_CurrentGoalIndex = 0;
        int onoff_status = 0;
        public TMP_Text on;
        public TMP_Text off;
        public TMP_Text question1;
        public TMP_Text question2;
        public TMP_Text keyword1;
        public TMP_Text keyword2;
        public GameObject extend_question1;
        public TMP_Text extend_question1_text;
        public GameObject extend_question2;
        public TMP_Text extend_question2_text;
        public GameObject extend_question3;
        public TMP_Text extend_question3_text;
        int[] order_list = new int[3];
        int question_refresh_times = 0;
        float time = 20;
        public static bool text_already;
        RectTransform rt;


        [Serializable]
        class Step
        {
            [SerializeField]
            public GameObject stepObject;

            [SerializeField]
            public string buttonText;

            public bool includeSkipButton;
        }

        [SerializeField]
        List<Step> m_StepList = new List<Step>();

        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField;

        [SerializeField]
        public GameObject m_SkipButton;
        public GameObject m_ContinueButton;

        [SerializeField]
        GameObject m_LearnButton;

        [SerializeField]
        GameObject m_LearnModal;

        [SerializeField]
        Button m_LearnModalButton;

        [SerializeField]
        GameObject m_CoachingUIParent;

        [SerializeField]
        FadeMaterial m_FadeMaterial;

        [SerializeField]
        Toggle m_PassthroughToggle;

        [SerializeField]
        LazyFollow m_GoalPanelLazyFollow;

        [SerializeField]
        GameObject m_TapTooltip;

        [SerializeField]
        GameObject m_VideoPlayer;

        [SerializeField]
        Toggle m_VideoPlayerToggle;

        [SerializeField]
        ARFeatureController m_FeatureController;

        [SerializeField]
        ObjectSpawner m_ObjectSpawner;

        const int k_NumberOfSurfacesTappedToCompleteGoal = 1;
        Vector3 m_TargetOffset = new Vector3(-.5f, -.25f, 1.5f);

        void Start()
        {

            m_OnboardingGoals = new Queue<Goal>();
            var welcomeGoal = new Goal(OnboardingGoals.Empty);
            var findSurfaceGoal = new Goal(OnboardingGoals.FindSurfaces);
            var tapSurfaceGoal = new Goal(OnboardingGoals.TapSurface);
            var endGoal = new Goal(OnboardingGoals.Empty);

            m_OnboardingGoals.Enqueue(welcomeGoal);
            m_OnboardingGoals.Enqueue(findSurfaceGoal);
            m_OnboardingGoals.Enqueue(tapSurfaceGoal);
            m_OnboardingGoals.Enqueue(endGoal);

            m_CurrentGoal = m_OnboardingGoals.Dequeue();
            if (m_TapTooltip != null)
                m_TapTooltip.SetActive(false);

            if (m_VideoPlayer != null)
            {
                m_VideoPlayer.SetActive(false);

                if (m_VideoPlayerToggle != null)
                    m_VideoPlayerToggle.isOn = false;
            }

            if (m_FadeMaterial != null)
            {
                m_FadeMaterial.FadeSkybox(false);

                if (m_PassthroughToggle != null)
                    m_PassthroughToggle.isOn = false;
            }

            if (m_LearnButton != null)
            {
                m_LearnButton.GetComponent<Button>().onClick.AddListener(OpenModal); ;
                m_LearnButton.SetActive(false);
            }

            if (m_LearnModal != null)
            {
                m_LearnModal.transform.localScale = Vector3.zero;
            }

            if (m_LearnModalButton != null)
            {
                m_LearnModalButton.onClick.AddListener(CloseModal);
            }

            if (m_ObjectSpawner == null)
            {
#if UNITY_2023_1_OR_NEWER
                m_ObjectSpawner = FindAnyObjectByType<ObjectSpawner>();
#else
                m_ObjectSpawner = FindObjectOfType<ObjectSpawner>();
#endif
            }

            if (m_FeatureController == null)
            {
#if UNITY_2023_1_OR_NEWER
                m_FeatureController = FindAnyObjectByType<ARFeatureController>();
#else
                m_FeatureController = FindObjectOfType<ARFeatureController>();
#endif
            }
        }

        void OpenModal()
        {
            if (m_LearnModal != null)
            {
                // m_LearnModal.transform.localScale = Vector3.one;
                if (question_refresh_times == 0)
                {
                    question_refresh_times += 1;
                    question1.text = WavSender.Q12;
                    question2.text = WavSender.Q22;
                    keyword1.text = WavSender.K1;
                    keyword2.text = WavSender.K2;
                    m_LearnButton.SetActive(false);
                }
            }
        }

        void CloseModal()
        {
            if (m_LearnModal != null)
            {
                // m_LearnModal.transform.localScale = Vector3.zero;

            }
        }

        void Update()
        {
            Debug.Log(time);
            if (time > 0)
            {
                time -= Time.deltaTime;
            }
            else
            {
                if (text_already)
                {
                    question1.text = WavSender.Q11;
                    question2.text = WavSender.Q21;
                    keyword1.text = WavSender.K1;
                    keyword2.text = WavSender.K2;
                    text_already = false;
                    time = 20;
                    question_refresh_times = 0;
                }
                if (onoff_status == 1 && question_refresh_times == 0)
                {
                    m_LearnButton.SetActive(true);
                }

            }
            if (!m_AllGoalsFinished)
            {
                ProcessGoals();
            }

            // Debug Input
#if UNITY_EDITOR
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                CompleteGoal();
            }
#endif
        }

        void ProcessGoals()
        {
            if (!m_CurrentGoal.Completed)
            {
                switch (m_CurrentGoal.CurrentGoal)
                {
                    case OnboardingGoals.Empty:
                        m_GoalPanelLazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
                        break;
                    case OnboardingGoals.FindSurfaces:
                        m_GoalPanelLazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
                        break;
                    case OnboardingGoals.TapSurface:
                        if (m_TapTooltip != null)
                        {
                            m_TapTooltip.SetActive(true);
                        }
                        m_GoalPanelLazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.None;
                        break;
                }
            }
        }

        void CompleteGoal()
        {
            // if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
            //     m_ObjectSpawner.objectSpawned -= OnObjectSpawned;

            // // disable tooltips before setting next goal
            // DisableTooltips();

            // m_CurrentGoal.Completed = true;
            // m_CurrentGoalIndex++;
            // if (m_OnboardingGoals.Count > 0)
            // {
            //     m_CurrentGoal = m_OnboardingGoals.Dequeue();
            //     m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);
            //     m_StepList[m_CurrentGoalIndex].stepObject.SetActive(true);
            //     m_StepButtonTextField.text = m_StepList[m_CurrentGoalIndex].buttonText;
            //     m_SkipButton.SetActive(m_StepList[m_CurrentGoalIndex].includeSkipButton);
            // }
            // else
            // {
            //     m_AllGoalsFinished = true;
            //     ForceEndAllGoals();
            // }

            // if (m_CurrentGoal.CurrentGoal == OnboardingGoals.FindSurfaces)
            // {
            //     if (m_FadeMaterial != null)
            //         m_FadeMaterial.FadeSkybox(true);

            //     if (m_PassthroughToggle != null)
            //         m_PassthroughToggle.isOn = true;

            //     if (m_LearnButton != null)
            //     {
            //         m_LearnButton.SetActive(true);
            //     }

            //     StartCoroutine(TurnOnPlanes(true));
            // }
            // else if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
            // {
            //     if (m_LearnButton != null)
            //     {
            //         m_LearnButton.SetActive(false);
            //     }
            //     m_SurfacesTapped = 0;
            //     m_ObjectSpawner.objectSpawned += OnObjectSpawned;
            // }
            // for (int i = 0; i < order_list.Length; i++)
            // {
            //     Debug.Log(order_list[i]);
            // }
            if (question1.text != "Loading...")
            {
                int count = order_list.Count(x => x == 0);
                if (count == 3)
                {
                    order_list[0] = 1;
                    extend_question1_text.text = question1.text;
                    extend_question1.SetActive(true);
                    rt = extend_question1.GetComponent<RectTransform>();
                    Vector2 pos = rt.anchoredPosition;
                    pos.y = 175f;
                    rt.anchoredPosition = pos;
                }
                else if (count == 2)
                {
                    if (order_list[0] == 1)
                    {
                        order_list[1] = 2;
                        extend_question2_text.text = question1.text;
                        extend_question2.SetActive(true);
                        rt = extend_question2.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 120f;
                        rt.anchoredPosition = pos;
                    }
                    else if (order_list[0] == 2 || order_list[0] == 3)
                    {
                        order_list[1] = 1;
                        extend_question1_text.text = question1.text;
                        extend_question1.SetActive(true);
                        rt = extend_question1.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 120f;
                        rt.anchoredPosition = pos;
                    }
                }
                else if (count == 1)
                {
                    if ((order_list[0] == 1 && order_list[1] == 2) || (order_list[0] == 2 && order_list[1] == 1))
                    {
                        order_list[2] = 3;
                        extend_question3_text.text = question1.text;
                        extend_question3.SetActive(true);
                        rt = extend_question3.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 65;
                        rt.anchoredPosition = pos;
                    }
                    else if ((order_list[0] == 1 && order_list[1] == 3) || (order_list[0] == 3 && order_list[1] == 1))
                    {
                        order_list[2] = 2;
                        extend_question2_text.text = question1.text;
                        extend_question2.SetActive(true);
                        rt = extend_question2.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 65;
                        rt.anchoredPosition = pos;
                    }
                    else if ((order_list[0] == 2 && order_list[1] == 3) || (order_list[0] == 3 && order_list[1] == 2))
                    {
                        order_list[2] = 1;
                        extend_question1_text.text = question1.text;
                        extend_question1.SetActive(true);
                        rt = extend_question1.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 65;
                        rt.anchoredPosition = pos;
                    }
                }
            }
            keyword1.text = "";
            question1.text = "Loading...";
            // Debug.Log(order_list[0]);
        }
        public void onoff()
        {
            if (onoff_status == 1)
            {
                onoff_status = 0;
                on.alpha = 0.5f;
                off.alpha = 1.0f;
                m_LearnButton.SetActive(false);
                m_SkipButton.SetActive(false);
                m_ContinueButton.SetActive(false);
                extend_question1.SetActive(false);
                extend_question2.SetActive(false);
                extend_question3.SetActive(false);
            }
            else
            {
                onoff_status = 1;
                on.alpha = 1.0f;
                off.alpha = 0.5f;
                if (question_refresh_times == 0)
                {
                    m_LearnButton.SetActive(true);
                }
                m_SkipButton.SetActive(true);
                m_ContinueButton.SetActive(true);
                if (Array.IndexOf(order_list, 1) != -1)
                {
                    extend_question1.SetActive(true);
                }
                if (Array.IndexOf(order_list, 2) != -1)
                {
                    extend_question2.SetActive(true);
                }
                if (Array.IndexOf(order_list, 3) != -1)
                {
                    extend_question3.SetActive(true);
                }
            }
        }
        public void Q1()
        {
            extend_question1.SetActive(false);
            int count = order_list.Count(x => x == 0);
            Debug.Log(count);
            if (count == 0)
            {
                if (Array.IndexOf(order_list, 1) == 0)
                {
                    order_list[0] = order_list[1];
                    order_list[1] = order_list[2];
                }
                else if (Array.IndexOf(order_list, 1) == 1)
                {
                    order_list[1] = order_list[2];
                }
                order_list[2] = 0;
                for (int i = 0; i < 2; i++)
                {
                    Debug.Log(order_list[i]);
                    if (order_list[i] == 1)
                    {
                        rt = extend_question1.GetComponent<RectTransform>();
                    }
                    else if (order_list[i] == 2)
                    {
                        rt = extend_question2.GetComponent<RectTransform>();
                    }
                    else if (order_list[i] == 3)
                    {
                        rt = extend_question3.GetComponent<RectTransform>();
                    }
                    if (i == 0)
                    {
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 175f;
                        rt.anchoredPosition = pos;
                    }
                    else if (i == 1)
                    {
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 120f;
                        rt.anchoredPosition = pos;
                    }
                }
            }
            else if (count == 1)
            {
                if (Array.IndexOf(order_list, 1) == 0)
                {
                    order_list[0] = order_list[1];
                }
                order_list[1] = 0;
                if (order_list[0] == 1)
                {
                    rt = extend_question1.GetComponent<RectTransform>();
                }
                else if (order_list[0] == 2)
                {
                    rt = extend_question2.GetComponent<RectTransform>();
                }
                else if (order_list[0] == 3)
                {
                    rt = extend_question3.GetComponent<RectTransform>();
                }
                Vector2 pos = rt.anchoredPosition;
                pos.y = 175f;
                rt.anchoredPosition = pos;
            }
            else if (count == 2)
            {
                order_list[0] = 0;
            }
        }
        public void Q2()
        {
            extend_question2.SetActive(false);
            int count = order_list.Count(x => x == 0);
            Debug.Log(count);
            if (count == 0)
            {
                if (Array.IndexOf(order_list, 2) == 0)
                {
                    order_list[0] = order_list[1];
                    order_list[1] = order_list[2];
                }
                else if (Array.IndexOf(order_list, 2) == 1)
                {
                    order_list[1] = order_list[2];
                }
                order_list[2] = 0;
                for (int i = 0; i < 2; i++)
                {
                    Debug.Log(order_list[i]);
                    if (order_list[i] == 1)
                    {
                        rt = extend_question1.GetComponent<RectTransform>();
                    }
                    else if (order_list[i] == 2)
                    {
                        rt = extend_question2.GetComponent<RectTransform>();
                    }
                    else if (order_list[i] == 3)
                    {
                        rt = extend_question3.GetComponent<RectTransform>();
                    }
                    if (i == 0)
                    {
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 175f;
                        rt.anchoredPosition = pos;
                    }
                    else if (i == 1)
                    {
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 120f;
                        rt.anchoredPosition = pos;
                    }
                }
            }
            else if (count == 1)
            {
                if (Array.IndexOf(order_list, 2) == 0)
                {
                    order_list[0] = order_list[1];
                }
                order_list[1] = 0;
                if (order_list[0] == 1)
                {
                    rt = extend_question1.GetComponent<RectTransform>();
                }
                else if (order_list[0] == 2)
                {
                    rt = extend_question2.GetComponent<RectTransform>();
                }
                else if (order_list[0] == 3)
                {
                    rt = extend_question3.GetComponent<RectTransform>();
                }
                Vector2 pos = rt.anchoredPosition;
                pos.y = 175f;
                rt.anchoredPosition = pos;
            }
            else if (count == 2)
            {
                order_list[0] = 0;
            }
        }
        public void Q3()
        {
            extend_question3.SetActive(false);
            int count = order_list.Count(x => x == 0);
            Debug.Log(count);
            if (count == 0)
            {
                if (Array.IndexOf(order_list, 3) == 0)
                {
                    order_list[0] = order_list[1];
                    order_list[1] = order_list[2];
                }
                else if (Array.IndexOf(order_list, 3) == 1)
                {
                    order_list[1] = order_list[2];
                }
                order_list[2] = 0;
                for (int i = 0; i < 2; i++)
                {
                    Debug.Log(order_list[i]);
                    if (order_list[i] == 1)
                    {
                        rt = extend_question1.GetComponent<RectTransform>();
                    }
                    else if (order_list[i] == 2)
                    {
                        rt = extend_question2.GetComponent<RectTransform>();
                    }
                    else if (order_list[i] == 3)
                    {
                        rt = extend_question3.GetComponent<RectTransform>();
                    }
                    if (i == 0)
                    {
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 175f;
                        rt.anchoredPosition = pos;
                    }
                    else if (i == 1)
                    {
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 120f;
                        rt.anchoredPosition = pos;
                    }
                }
            }
            else if (count == 1)
            {
                if (Array.IndexOf(order_list, 3) == 0)
                {
                    order_list[0] = order_list[1];
                }
                order_list[1] = 0;
                if (order_list[0] == 1)
                {
                    rt = extend_question1.GetComponent<RectTransform>();
                }
                else if (order_list[0] == 2)
                {
                    rt = extend_question2.GetComponent<RectTransform>();
                }
                else if (order_list[0] == 3)
                {
                    rt = extend_question3.GetComponent<RectTransform>();
                }
                Vector2 pos = rt.anchoredPosition;
                pos.y = 175f;
                rt.anchoredPosition = pos;
            }
            else if (count == 2)
            {
                order_list[0] = 0;
            }
        }

        public IEnumerator TurnOnPlanes(bool visualize)
        {
            yield return new WaitForSeconds(1f);

            if (m_FeatureController != null)
            {
                m_FeatureController.TogglePlaneVisualization(visualize);
                m_FeatureController.TogglePlanes(true);
            }
        }

        IEnumerator TurnOnARFeatures()
        {
            if (m_FeatureController == null)
                yield return null;

            yield return new WaitForSeconds(0.5f);

            // We are checking the bounding box count here so that we disable plane visuals so there is no
            // visual Z fighting. If the user has not defined any furniture in space setup or the platform
            // does not support bounding boxes, we want to enable plane visuals, but disable bounding box visuals.
            m_FeatureController.ToggleBoundingBoxes(true);
            m_FeatureController.TogglePlanes(true);

            // Quick hack for for async await race condition.
            // TODO: -- Probably better to listen to trackable change events in the ARFeatureController and update accordingly there
            yield return new WaitForSeconds(0.5f);
            m_FeatureController.ToggleDebugInfo(false);

            // If there are bounding boxes, we want to hide the planes so they don't cause z-fighting.
            if (m_FeatureController.HasBoundingBoxes())
            {
                m_FeatureController.TogglePlaneVisualization(false);
                m_FeatureController.ToggleBoundingBoxVisualization(true);
            }
            else
            {
                m_FeatureController.ToggleBoundingBoxVisualization(true);
            }

            m_FeatureController.occlusionManager.SetupManager();
        }

        void TurnOffARFeatureVisualization()
        {
            if (m_FeatureController == null)
                return;

            m_FeatureController.TogglePlaneVisualization(false);
            m_FeatureController.ToggleBoundingBoxVisualization(false);
        }

        void DisableTooltips()
        {
            if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
            {
                if (m_TapTooltip != null)
                {
                    m_TapTooltip.SetActive(false);
                }
            }
        }

        public void ForceCompleteGoal()
        {
            CompleteGoal();
        }

        public void ForceEndAllGoals()
        {
            // m_CoachingUIParent.transform.localScale = Vector3.zero;

            // TurnOnVideoPlayer();

            // if (m_VideoPlayerToggle != null)
            //     m_VideoPlayerToggle.isOn = true;

            // if (m_FadeMaterial != null)
            // {
            //     m_FadeMaterial.FadeSkybox(true);

            //     if (m_PassthroughToggle != null)
            //         m_PassthroughToggle.isOn = true;
            // }

            // if (m_LearnButton != null)
            // {
            //     m_LearnButton.SetActive(false);
            // }

            // if (m_LearnModal != null)
            // {
            //     m_LearnModal.transform.localScale = Vector3.zero;
            // }

            // StartCoroutine(TurnOnARFeatures());
            if (question2.text != "Loading...")
            {
                int count = order_list.Count(x => x == 0);
                if (count == 3)
                {
                    order_list[0] = 1;
                    extend_question1_text.text = question2.text;
                    extend_question1.SetActive(true);
                    rt = extend_question1.GetComponent<RectTransform>();
                    Vector2 pos = rt.anchoredPosition;
                    pos.y = 175f;
                    rt.anchoredPosition = pos;
                }
                else if (count == 2)
                {
                    if (order_list[0] == 1)
                    {
                        order_list[1] = 2;
                        extend_question2_text.text = question2.text;
                        extend_question2.SetActive(true);
                        rt = extend_question2.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 120f;
                        rt.anchoredPosition = pos;
                    }
                    else if (order_list[0] == 2 || order_list[0] == 3)
                    {
                        order_list[1] = 1;
                        extend_question1_text.text = question2.text;
                        extend_question1.SetActive(true);
                        rt = extend_question1.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 120f;
                        rt.anchoredPosition = pos;
                    }
                }
                else if (count == 1)
                {
                    if ((order_list[0] == 1 && order_list[1] == 2) || (order_list[0] == 2 && order_list[1] == 1))
                    {
                        order_list[2] = 3;
                        extend_question3_text.text = question2.text;
                        extend_question3.SetActive(true);
                        rt = extend_question3.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 65;
                        rt.anchoredPosition = pos;
                    }
                    else if ((order_list[0] == 1 && order_list[1] == 3) || (order_list[0] == 3 && order_list[1] == 1))
                    {
                        order_list[2] = 2;
                        extend_question2_text.text = question2.text;
                        extend_question2.SetActive(true);
                        rt = extend_question2.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 65;
                        rt.anchoredPosition = pos;
                    }
                    else if ((order_list[0] == 2 && order_list[1] == 3) || (order_list[0] == 3 && order_list[1] == 2))
                    {
                        order_list[2] = 1;
                        extend_question1_text.text = question2.text;
                        extend_question1.SetActive(true);
                        rt = extend_question1.GetComponent<RectTransform>();
                        Vector2 pos = rt.anchoredPosition;
                        pos.y = 65;
                        rt.anchoredPosition = pos;
                    }
                }
            }
            question2.text = "Loading...";
            keyword2.text = "";
        }

        public void ResetCoaching()
        {
            TurnOffARFeatureVisualization();
            m_CoachingUIParent.transform.localScale = Vector3.one;

            m_OnboardingGoals.Clear();
            m_OnboardingGoals = new Queue<Goal>();
            var welcomeGoal = new Goal(OnboardingGoals.Empty);
            var findSurfaceGoal = new Goal(OnboardingGoals.FindSurfaces);
            var tapSurfaceGoal = new Goal(OnboardingGoals.TapSurface);
            var endGoal = new Goal(OnboardingGoals.Empty);

            m_OnboardingGoals.Enqueue(welcomeGoal);
            m_OnboardingGoals.Enqueue(findSurfaceGoal);
            m_OnboardingGoals.Enqueue(tapSurfaceGoal);
            m_OnboardingGoals.Enqueue(endGoal);

            for (int i = 0; i < m_StepList.Count; i++)
            {
                if (i == 0)
                {
                    m_StepList[i].stepObject.SetActive(true);
                    m_SkipButton.SetActive(m_StepList[i].includeSkipButton);
                    m_StepButtonTextField.text = m_StepList[i].buttonText;
                }
                else
                {
                    m_StepList[i].stepObject.SetActive(false);
                }
            }

            m_CurrentGoal = m_OnboardingGoals.Dequeue();
            m_AllGoalsFinished = false;

            if (m_TapTooltip != null)
                m_TapTooltip.SetActive(false);

            if (m_LearnButton != null)
            {
                m_LearnButton.SetActive(false);
            }

            if (m_LearnModal != null)
            {
                m_LearnModal.transform.localScale = Vector3.zero;
            }

            m_CurrentGoalIndex = 0;
        }

        void OnObjectSpawned(GameObject spawnedObject)
        {
            m_SurfacesTapped++;
            if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface && m_SurfacesTapped >= k_NumberOfSurfacesTappedToCompleteGoal)
            {
                CompleteGoal();
                m_GoalPanelLazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
            }
        }

        public void TooglePlayer(bool visibility)
        {
            if (visibility)
            {
                TurnOnVideoPlayer();
            }
            else
            {
                if (m_VideoPlayer.activeSelf)
                {
                    m_VideoPlayer.SetActive(false);
                    if (m_VideoPlayerToggle.isOn)
                        m_VideoPlayerToggle.isOn = false;
                }
            }
        }

        void TurnOnVideoPlayer()
        {
            if (m_VideoPlayer.activeSelf)
                return;

            var follow = m_VideoPlayer.GetComponent<LazyFollow>();
            if (follow != null)
                follow.rotationFollowMode = LazyFollow.RotationFollowMode.None;

            m_VideoPlayer.SetActive(false);
            var target = Camera.main.transform;
            var targetRotation = target.rotation;
            var newTransform = target;
            var targetEuler = targetRotation.eulerAngles;
            targetRotation = Quaternion.Euler
            (
                0f,
                targetEuler.y,
                targetEuler.z
            );

            newTransform.rotation = targetRotation;
            var targetPosition = target.position + newTransform.TransformVector(m_TargetOffset);
            m_VideoPlayer.transform.position = targetPosition;

            var forward = target.position - m_VideoPlayer.transform.position;
            var targetPlayerRotation = forward.sqrMagnitude > float.Epsilon ? Quaternion.LookRotation(forward, Vector3.up) : Quaternion.identity;
            targetPlayerRotation *= Quaternion.Euler(new Vector3(0f, 180f, 0f));
            var targetPlayerEuler = targetPlayerRotation.eulerAngles;
            var currentEuler = m_VideoPlayer.transform.rotation.eulerAngles;
            targetPlayerRotation = Quaternion.Euler
            (
                currentEuler.x,
                targetPlayerEuler.y,
                currentEuler.z
            );

            m_VideoPlayer.transform.rotation = targetPlayerRotation;
            m_VideoPlayer.SetActive(true);
            if (follow != null)
                follow.rotationFollowMode = LazyFollow.RotationFollowMode.LookAtWithWorldUp;
        }
    }
}
