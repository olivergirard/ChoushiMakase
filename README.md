# ChoushiMakase
ChoushiMakase is a plugin for the singing synthesizer application [*UTAU*](https://utau2008.xrea.jp/) that maps changes in audio frequency to control points used to create synthetic note pitchbends. This plugin is not intended to be a complete replacement for manual note tuning, but rather an auxiliary tool to help users visualize and understand how to replicate real-life pitchbends in *UTAU*. This plugin makes use of the [NAudio](https://github.com/naudio/NAudio), [FftSharp](https://github.com/swharden/FftSharp), and [UtauPlugin](https://github.com/delta-kimigatame/utauPlugin) libraries and would not be possible without the contributions of these developers.

## Execution
To add the plugin to your *UTAU* application, place the `bin\Debug` folder in your `UTAU\plugins` directory and rename it as needed. 

To run the plugin, open a new `.ust` file in *UTAU*, select the notes that correspond to those in the audio file you would like to collect frequency data from, and click `Tools(T) > Plug-Ins(N) > ChoushiMakase` to run the console.

<p align="center">
  <img src="https://github.com/user-attachments/assets/47730efd-72d2-442c-98a9-7f8c17d5bd25" alt="Tools(T) > Plug-Ins(N) > ChoushiMakase."/>
</p>

You will be prompted for the path of the audio file, which must be an `.mp3` or a `.wav` file; an absolute path collected through the `Copy as path` command is acceptable. The console will close automatically upon program completion.

## Example Usage
TODO
