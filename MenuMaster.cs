using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PlanetTaxi
{
    public class MenuMaster : MonoBehaviour
    {
        [Tooltip("Háttérzene")]
        [SerializeField] private AudioSource _backgroundMusic;
        [Tooltip("Töltőképernyő szülőobjektuma")]
        [SerializeField] private GameObject _loadingObjects;
        [Tooltip("Töltésjelző")]
        [SerializeField] private Slider _loadingSlider;
        [Tooltip("Mentett játék folytatása gomb")] 
        [SerializeField] private Button _continueGameButton;

        private bool _loadingObjectsFilled, _loadingSliderFilled;
	
        private void Start()
        {
            _loadingObjectsFilled = _loadingObjects != null;
            _loadingSliderFilled = _loadingSlider != null;
		
            if (_backgroundMusic != null && !_backgroundMusic.isPlaying) _backgroundMusic.Play();
            if (_loadingObjectsFilled) _loadingObjects.SetActive(false);
            _continueGameButton.interactable = PlayerPrefs.HasKey("CurrentLevel");

        }

        public void StartNewGame(string levelName)
        {
            PlayerPrefs.DeleteKey("CurrentLevel");
            PlayerPrefs.DeleteKey("FullPay");
            if (_loadingObjectsFilled) _loadingObjects.SetActive(true);
            StartCoroutine(LoadLevel(levelName));
        }

        public void ContinueGame()
        {
            if (_loadingObjectsFilled) _loadingObjects.SetActive(true);
            StartCoroutine(LoadLevel(PlayerPrefs.GetString("CurrentLevel")));
        }

        private IEnumerator LoadLevel(string levelName)
        {
            AsyncOperation loading = SceneManager.LoadSceneAsync(levelName);
            loading.allowSceneActivation = false;
		
            while (loading.progress < 0.9f)
            {
                if (_loadingSliderFilled) _loadingSlider.value = loading.progress;
                yield return new WaitForSecondsRealtime(0.1f);
            }

            if (_loadingSliderFilled) _loadingSlider.value = 1.0f;
            loading.allowSceneActivation = true;
        }

        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
