# AI-Guided Voice-Interactive VR Meditation

**MSc Thesis — Media Technology, Technische Universität Ilmenau (2025–2026)**

An immersive VR meditation system integrating AI-generated voice guidance, physiological biofeedback, and adaptive audio-visual environments to study meditation effectiveness.

## Key Results

- VR significantly improved **user engagement and satisfaction**
- Comparable physiological relaxation across both meditation modalities
- N=44 controlled user study

## Tech Stack

| Area                  | Technologies                                     |
| --------------------- | ------------------------------------------------ |
| VR Development        | Unity, SteamVR, HTC Vive Pro                     |
| AI Integration        | OpenAI API, Text-to-Speech                       |
| Data Analysis         | Python, pandas, NumPy, SciPy, Matplotlib         |
| Physiological Sensors | Biofeedback 2000 Xpert (HR, HRV, SCL, Skin Temp) |
| Statistics            | Wilcoxon signed-rank, Mann-Whitney U             |

## Project Structure

├── Unity/ # VR application source code (C#)
├── Analysis/ # Python data analysis scripts
├── Data/ # Anonymised study data
└── Results/ # Visualisations and statistical outputs

## Note on Large Assets

Large third-party Unity assets (HDRI textures, audio, Oculus plugins) are excluded due to GitHub's 2GB limit. All Python scripts and Unity C# source code are fully included. Contact author for the complete package.

## Run Data Analysis

```bash
pip install pandas numpy scipy matplotlib
python Analysis/hrv_analysis.py
```

## Author

**Muhammad Abid Zahid** — MSc Media Technology, TU Ilmenau

[LinkedIn](https://linkedin.com/in/muhammad-abid-zahid-bbb991176) | [GitHub](https://github.com/MAZahid96)
