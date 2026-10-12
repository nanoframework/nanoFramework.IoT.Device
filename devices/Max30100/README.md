# MAX30100 pulse oximeter and heart-rate sensor

The MAX30100 integrates red and infrared LEDs, a photodetector, optical elements, and low-noise electronics for pulse-oximetry and heart-rate signal acquisition. This binding configures the sensor and reads its raw red and infrared FIFO samples.

## Documentation

- [MAX30100 datasheet](https://www.analog.com/media/en/technical-documentation/data-sheets/MAX30100.pdf)

## Hardware required

- ESP32 development board
- MAX30100 breakout module
- Jumper wires

## ESP32 wiring

| MAX30100 | ESP32 |
| --- | --- |
| VIN | 3.3 V |
| GND | GND |
| SDA | GPIO21 |
| SCL | GPIO22 |
| INT | Not required by this sample |

Use 3.3 V logic. Some breakout boards include voltage regulators and I2C pull-ups; check the specifications of your module before connecting it.

## Usage

Configure the ESP32 I2C pins before creating the I2C device:

```csharp
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Max30100.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);
Max30100 sensor = new Max30100(i2cDevice);
Max30100PulseOximeterOptions pulseOximeterOptions = new Max30100PulseOximeterOptions
{
    SampleRate = 50,
    WindowSeconds = 8,
};
Max30100PulseOximeterWorker pulseOximeter = new Max30100PulseOximeterWorker(
    pulseOximeterOptions,
    queueSeconds: 2);

sensor.Reset();
sensor.Configure(
    Max30100Mode.SpO2,
    Max30100SampleRate.SamplesPerSecond50,
    Max30100PulseWidth.Microseconds1600,
    Max30100LedCurrent.Milliamps27Point1,
    Max30100LedCurrent.Milliamps37);

uint processedDroppedSamples = 0;

while (true)
{
    while (sensor.TryReadSample(out Max30100Reading reading))
    {
        if (sensor.DroppedSampleCount != processedDroppedSamples)
        {
            processedDroppedSamples = sensor.DroppedSampleCount;
            pulseOximeter.Reset();
        }

        pulseOximeter.AddSample(reading);
    }

    Thread.Sleep(2);
}
```

Always drain all available FIFO samples before sleeping. At the sample's 50 samples per second, the 16-sample FIFO contains 320 milliseconds of data. Writing every raw sample to the debugger can be slower than acquisition and cause overflow. The driver batches FIFO-status reads, recovers from a full FIFO, and exposes the cumulative number of discarded samples through `DroppedSampleCount`. The complete sample prepares its worker and diagnostic threads before enabling sensor sampling, gives the acquisition thread the highest priority, gives its diagnostic thread the lowest priority, and limits output to once every four seconds. The shortened acquisition loop above intentionally omits diagnostic output.

`Max30100PulseOximeterWorker` queues samples in a fixed ring buffer and performs filtering, averaging, and measurement calculations on a background thread. This keeps the I2C reading loop responsive while analysis is running. Its `DroppedSampleCount` reports samples discarded if the processing queue cannot keep up. `Reset()` is asynchronous, and `Dispose()` stops and joins the worker thread.

Use `Max30100PulseOximeter` directly when synchronous processing is preferred. Both processor types use fixed buffers and do not allocate per sample.

The sample rate passed to either processor must exactly match the sensor sample rate. A mismatch changes the filter frequencies, analysis-window duration, and calculated BPM. For example, configuring the sensor for 100 samples per second while constructing the processor for 50 samples per second makes the calculated pulse rate approximately half its actual value.

## Measurement configuration

The sample uses:

```csharp
sensor.Configure(
    Max30100Mode.SpO2,
    Max30100SampleRate.SamplesPerSecond50,
    Max30100PulseWidth.Microseconds1600,
    Max30100LedCurrent.Milliamps27Point1,
    Max30100LedCurrent.Milliamps37);
```

### Measurement mode

`Max30100Mode.SpO2` enables both the red and infrared channels. Both channels are required for the experimental oxygen-saturation ratio. Heart-rate processing primarily uses infrared data. `Max30100Mode.HeartRate` uses only the infrared channel and is appropriate when oxygen saturation is not needed.

### Sample rate

The ESP32 sample uses `SamplesPerSecond50`, which records a sample every 20 milliseconds and gives the 16-sample FIFO 320 milliseconds of capacity. This is sufficient for the supported pulse-rate range and is the recommended starting point when debugger output is enabled.

`SamplesPerSecond100` records a sample every 10 milliseconds and provides finer beat-interval resolution, but it:

- Doubles I2C traffic and signal-processing work.
- Halves FIFO time capacity from 320 milliseconds to 160 milliseconds.
- Increases the chance of FIFO overflow during debugging or lengthy processing.
- Doubles the processor working-buffer memory for the same analysis-window duration.

Use 100 samples per second only when the application can guarantee sufficiently responsive acquisition. Always use the same rate when constructing `Max30100PulseOximeter`.

### LED pulse width

`Microseconds1600` selects a 1600-microsecond LED pulse and 16-bit ADC resolution. It is valid with the sample's 50-sample-per-second rate. Compared with `Microseconds800` and its 15-bit resolution, it:

- Uses a longer optical integration period and more LED energy per sample.
- Provides twice the numerical ADC range and one additional bit of resolution.
- Improves sensitivity when the pulsatile signal is weak.
- Requires enough ADC headroom to accommodate the increased optical response.

The effective full-scale values are 32,767 at 800 microseconds and 65,535 at 1600 microseconds. As a practical starting point, keep stable finger-contact levels around 30% through 70% of full scale:

| Pulse width | Resolution | Full scale | Suggested stable range |
| --- | --- | --- | --- |
| 800 microseconds | 15 bit | 32,767 | Approximately 10,000–23,000 |
| 1600 microseconds | 16 bit | 65,535 | Approximately 20,000–46,000 |

With a 1600-microsecond pulse, IR around 32,000 and red around 28,000 are near the middle of the ADC range and have adequate headroom. Quality 0 at those levels indicates that the filtered AC waveform did not contain enough consistent beat intervals; it does not indicate insufficient DC brightness.

### LED currents

The sample drives the red LED at 27.1 mA and the infrared LED at 37.0 mA. Increasing current generally raises both the DC reflection and pulsatile AC signal, but it also increases power consumption, LED heating, and the risk of ADC clipping. It does not necessarily improve perfusion index because both AC and DC can rise together. The red current provides more signal than 24 mA while retaining more headroom than 37 mA, which can clip at 65,535 on some fingers and modules.

The chosen unequal currents compensate for different red and infrared optical response. Tune each channel independently:

- Increase current when the raw level and pulsatile amplitude are too low.
- Decrease current when readings flatten near their upper range or stop responding to finger-pressure changes.
- Keep enough headroom for the pulse waveform and movement transients.
- Prefer a stable, unclipped waveform over making both channel values numerically equal.

Changing pulse width changes the effective optical energy, so LED currents may need retuning after switching between 800 and 1600 microseconds.

The sample processor first discards two seconds while finger pressure and optical levels stabilize, then uses an eight-second window of uninterrupted finger data. It restarts collection after an abrupt red or infrared level change because that indicates finger motion. It normally requires two consistent pulse candidates before `HasValidHeartRate` can become true. A candidate with at least four consistent beat intervals and 80% quality can be accepted immediately. A candidate survives up to two weak analysis windows, and finger removal is debounced across five samples. Reset the processor after any FIFO overflow because missing samples invalidate the waveform timing:

- `HeartRate` fuses hysteresis beat intervals with normalized autocorrelation of the filtered infrared waveform. Median interval filtering rejects outliers and compensates for isolated missed beats, while autocorrelation remains stable when waveform shape causes missing or extra threshold crossings.
- `OxygenSaturation` uses an uncalibrated ratio-of-ratios approximation.
- `PerfusionIndex` is the infrared AC signal as a percentage of its DC level.
- `SignalQuality` combines beat-interval consistency, the amount of interval evidence, and the proportion of raw intervals accepted. Two matching intervals can no longer report 100% quality, and windows containing several rejected intervals are penalized. At least four accepted intervals are needed to reach the full consistency score. Heart-rate readings below 45% are rejected.
- `HasValidHeartRate` reports infrared pulse validity independently of the red-channel SpO₂ calculation.
- `IsHeartRateHeld` indicates that the last confirmed heart rate is being retained through a weak or disagreeing analysis window. A held value is discarded after three consecutive misses or immediately after a signal reset.
- `HasValidOxygenSaturation` reports whether the experimental ratio-of-ratios result is available.
- `UsedWeakSignalOxygenSaturation` identifies SpO₂ values accepted through repeated weak-signal stability checks.
- `CandidateHeartRate` exposes the current unconfirmed pulse candidate for diagnostics.
- `AnalysisWindowProgress` distinguishes expected zero diagnostics while the stable-sample window is still filling from an analyzed window that contains no usable pulse.
- `InfraredAcRms` reports the detrended infrared pulse amplitude in ADC counts.
- `RedPerfusionIndex` reports the red AC signal relative to its DC level.
- `ChannelCorrelation` reports whether the detrended red and infrared pulse shapes agree. Experimental SpO₂ requires correlation of at least 0.80 and at least 0.10% perfusion in both channels.
- `RatioOfRatios` reports `(red AC / red DC) / (infrared AC / infrared DC)`. The demonstration converts it with `110 - 25R`, but accurate conversion coefficients depend on the sensor module, optics, finger placement, and calibration against reference measurements.
- `OxygenSaturationCalibrationIntercept` and `OxygenSaturationCalibrationSlope` expose the active coefficients in `SpO₂ = intercept + slope × ratio`.
- `BeatDetectionThreshold` reports the positive and negative hysteresis level used to detect beats.
- `AutocorrelationHeartRate` independently reports the strongest prominent periodic waveform in the supported heart-rate range.
- `AutocorrelationQuality` reports periodic peak strength, prominence over its shoulders, and whether the window contains enough cycles.
- `HeartRateEstimatorsDisagree` identifies windows rejected because valid interval and periodic estimates differ by more than 12 BPM.
- `UsedAutocorrelationFallback` identifies candidates obtained from strong periodicity when threshold-crossing intervals were unusable.
- `DetectedBeatCrossings` reports how many complete negative-to-positive threshold crossings were found.
- `RawBeatIntervals` reports how many crossings produced an interval in the supported 40–180 BPM range.
- `DetectedBeatIntervals` reports how many raw intervals passed consistency filtering. At least four are required for a candidate and full interval evidence.
- `FingerDetected` indicates whether both optical channels exceed the contact threshold.
- `IsSettling` indicates that initial finger-placement samples are still being discarded.

Movement, changing finger pressure, ambient light, clipping near the selected pulse width's full scale, or FIFO overflow can invalidate the result. Adjust the red and infrared LED currents if either channel regularly clips or has an inadequate signal.

Motion rejection treats a one-sample red or infrared DC change above 4% as abrupt movement. A minimum threshold of 750 ADC counts prevents ordinary low-level noise from restarting collection. These are empirical processing thresholds rather than MAX30100 register or datasheet values. Both can be configured when constructing either processor:

```csharp
Max30100PulseOximeterOptions options = new Max30100PulseOximeterOptions
{
    SampleRate = 100,
    WindowSeconds = 8,
    AbruptChangeRelativePercent = 5,
    MinimumAbruptChangeCounts = 1000,
    MinimumHeartRate = 40,
    MaximumHeartRate = 180,
    MinimumHeartRateQuality = 45,
    MinimumAutocorrelationQuality = 55,
    AnalysisIntervalSeconds = 2,
};

Max30100PulseOximeterWorker pulseOximeter = new Max30100PulseOximeterWorker(
    options,
    queueSeconds: 2,
    publishRate: 10);
```

`Max30100PulseOximeterOptions` initializes every property to the recommended default. Only properties that need tuning have to be assigned. `new Max30100PulseOximeter()` and `new Max30100PulseOximeterWorker()` use the same default options automatically.

Increasing either value makes motion detection less sensitive and avoids unnecessary restarts, but allows larger movement artifacts into the analysis window. Decreasing either value rejects smaller disturbances but can repeatedly restart collection because of normal optical noise. A value of zero disables that threshold component; setting both to zero treats every nonzero sample change as abrupt movement.

`HeartRateCandidateTolerance` defaults to 12 BPM. Estimates within the tolerance are smoothed into the established candidate. A larger difference is tracked as an alternate candidate and must repeat before replacement. Lower values resist sudden jumps but follow real heart-rate changes more slowly; higher values follow changes faster but can merge noise or harmonic estimates into the current rate.

`HeartRateCandidateMissLimit` defaults to three analysis windows. A confirmed value is marked as held for up to `missLimit - 1` consecutive invalid windows and is cleared when the limit is reached. At the default two-second analysis interval, a limit of three holds through approximately four seconds of weak analysis and clears on the analysis around six seconds. Signal resets such as finger removal, abrupt motion, or dropped samples still clear immediately.

The remaining high-value processing thresholds are configurable through either processor constructor:

- `FingerDetectionThreshold` sets the raw ADC contact threshold for both channels. Increase it to reject ambient-light and no-finger baselines, or decrease it for modules with lower optical output.
- `MinimumHeartRate` and `MaximumHeartRate` define the accepted BPM range and the interval/autocorrelation search range.
- `HeartRateEstimatorAgreementTolerance` sets how closely interval and autocorrelation BPM estimates must agree.
- `MinimumHeartRateQuality` and `MinimumAutocorrelationQuality` reject weak interval and periodic estimates. Higher values are stricter.
- `MinimumBeatIntervalCount` controls the amount of consistent interval evidence required. It can be set from 2 through 20.
- `StrongCandidateQuality` controls when a candidate with enough intervals can be accepted immediately instead of waiting for another analysis window.
- `MinimumHeartRatePerfusionIndex` rejects infrared pulse signals whose AC amplitude is too small relative to their DC level.
- `MinimumOxygenSaturationPerfusionIndex`, `MinimumOxygenSaturationCorrelation`, `MinimumOxygenSaturationRatio`, and `MaximumOxygenSaturationRatio` gate the experimental SpO₂ estimate.

These are empirical defaults, not medical limits or MAX30100 register values. Loosening them can produce readings sooner but increases false or unstable results. Tightening them improves rejection but can make readings unavailable on weak signals.

When both channel perfusion indexes are below the normal 0.10% threshold but remain at least 0.04%, the processor can use adaptive weak-signal validation. It requires correlation of at least 0.90 and a ratio that changes by no more than 0.10 for three consecutive analysis windows. This allows stable low-amplitude signals without treating a single weak window as valid. The thresholds are configured with `MinimumWeakSignalOxygenSaturationPerfusionIndex`, `MinimumWeakSignalOxygenSaturationCorrelation`, `WeakSignalOxygenSaturationRatioTolerance`, and `WeakSignalOxygenSaturationConfirmationCount`. Weak-signal values remain experimental and are marked by `UsedWeakSignalOxygenSaturation`.

When beat detection temporarily fails but infrared perfusion remains above the heart-rate threshold and red/infrared correlation remains valid, `StableOpticalSignalHeartRateMissLimit` holds the last confirmed heart rate for up to six analysis windows instead of the normal three. This keeps SpO₂ available through short waveform-shape changes while exposing the BPM as held. Finger removal, abrupt motion, FIFO loss, or loss of optical coherence still resets the signal immediately or uses the shorter normal miss limit.

Filter and beat-detection heuristics are also configurable:

- `BaselineTrackingRate` controls how quickly the baseline follows slow optical-level changes. Increasing it rejects faster drift but can remove part of a slow pulse waveform.
- `SignalSmoothingRate` controls how quickly the smoothed AC signal follows the baseline-removed input. Increasing it preserves faster detail and noise; decreasing it produces stronger smoothing.
- `FilterSettlingSeconds` controls how much of the start of each filtered window is excluded while filter state converges.
- `BeatDetectionThresholdFraction` sets the symmetric hysteresis threshold as a fraction of infrared AC RMS. Increasing it rejects small fluctuations but may miss weak beats.
- `MinimumBeatDetectionThreshold` prevents near-zero RMS windows from producing crossings from numerical or ADC noise.
- `MinimumAcRms` is the minimum infrared amplitude for heart rate and minimum red amplitude for SpO₂.
- `MissedBeatIntervalFactor` identifies unusually long intervals that are divided by two to compensate for one missed crossing.
- `BeatIntervalToleranceFraction` controls how far an interval may differ from the median before it is rejected.

Filter rates are normalized by the configured sample rate internally, so the defaults retain comparable behavior at supported sensor sample rates. These parameters describe processing behavior rather than exact analog cutoff frequencies.

Contact and processing timing can be tuned independently:

- `FingerLossSignalResetSamples` clears partial waveform analysis after consecutive no-contact samples.
- `FingerRemovalDebounceSamples` confirms complete removal and must be greater than the signal-reset count.
- `InitialFingerSettlingSeconds` discards pressure and optical transients after initial contact.
- `MotionRecoverySettlingSeconds` discards samples after abrupt motion or temporary contact loss.
- `AnalysisIntervalSeconds` controls how often a full analysis runs after the window is filled. Shorter intervals improve responsiveness but increase processing load.

Candidate confirmation and smoothing are configurable:

- `HeartRateConfirmationCount` controls how many matching normal candidates are needed.
- `AlternateHeartRateConfirmationCount` controls how many matching candidates are needed to replace an established rate after a large change.
- `HeartRateCandidateSmoothingFactor`, `HeartRateSmoothingFactor`, and `OxygenSaturationSmoothingFactor` are the weights of the newest values. A value of one disables smoothing; smaller values respond more slowly.

Estimator fusion can be adjusted with `MinimumAutocorrelationChannelCorrelation`, `EstimatorDisagreementQualityFactor`, and `AutocorrelationFallbackQualityFactor`. The correlation threshold gates periodic fallback. The quality factors reduce confidence when estimators disagree or when only autocorrelation supplies a candidate.

`IntervalDeviationQualityPenalty` controls how strongly beat-interval variation reduces quality. The interval-evidence score uses `MinimumBeatIntervalCount`, so changing the required interval count also changes the amount of evidence needed for a full score.

Autocorrelation behavior is configurable:

- `AutocorrelationTargetSampleRate` controls decimation before autocorrelation. Lower rates reduce processing work but provide coarser lag resolution.
- `AutocorrelationHarmonicThreshold` controls how strong the half-lag peak must be before it replaces a likely two-beat period.
- `AutocorrelationShoulderFraction` controls how far from a peak its prominence is measured.
- `AutocorrelationProminenceForFullQuality` controls the prominence needed for full prominence evidence.
- `AutocorrelationCyclesForFullQuality` controls how many waveform cycles are needed for full cycle evidence.

The worker's `publishRate` constructor argument controls how many state snapshots are requested per second. Lower values reduce locking and copying; higher values make raw values and progress more responsive. The actual interval is an integer number of sensor samples and can therefore be slightly slower than the requested rate.

The processor removes residual linear drift from each filtered analysis window before calculating perfusion and detecting beats. This prevents a gradual pressure or temperature trend from inflating perfusion and producing a small number of misleading low-rate crossings.

The processor defaults to a six-second analysis window, and the ESP32 sample uses an eight-second window. The additional interval evidence makes it less likely that a short noise burst or harmonic will replace an established BPM candidate. Candidates based on only three intervals are rejected. A candidate more than 12 BPM from the existing candidate must also repeat before it replaces that candidate.

The interval and autocorrelation estimates must normally agree within 12 BPM. If interval detection is unusable but autocorrelation has at least 55% confidence and red/infrared correlation is at least 0.75, the periodic estimate can be used at reduced quality and still requires normal multi-window candidate confirmation. When both estimators are individually valid but disagree, that analysis window is rejected instead of selecting one arbitrarily.

After a heart rate has been confirmed, up to two weak or disagreeing windows retain the last value and expose `IsHeartRateHeld`. This prevents display flicker while making stale data explicit. A third consecutive miss clears the result.

Autocorrelation is evaluated on a waveform decimated to approximately 25 samples per second. Pulse frequencies remain well below that rate, while the reduced work prevents signal analysis from starving FIFO acquisition on constrained devices. Parabolic peak interpolation recovers finer BPM resolution than the decimated lag spacing, and half-rate harmonic correction prefers a credible one-beat period when the strongest peak spans two beats. The worker defaults to a two-second processing queue. A rising processing dropped-sample count means the processor still cannot keep up; those losses reset analysis because waveform timing is no longer continuous.

`ReadTemperature()` returns a UnitsNet `Temperature` containing the MAX30100 internal die temperature. It is not a body, skin, or ambient-temperature measurement:

```csharp
UnitsNet.Temperature dieTemperature = sensor.ReadTemperature();
Debug.WriteLine($"Sensor die temperature: {dieTemperature.DegreesCelsius:F2} °C");
```

## Typical output from the sample

This is typically what you can get from the sample. Collecting can take a bit of time but if you are stuck in this, move your finger or use another one. Hold without moving the finger on the sensor.

```csharp
MAX30100 part ID: 0x11, revision: 0x05
Place a finger on the sensor.
Place a finger on the sensor.
Stabilizing finger contact. IR: 44153, red: 47963.
Collecting a stable signal. IR: 44148, red: 47780, perfusion: 0.00%, red perfusion: 0.00%, correlation: 0.00, ratio: 0.00, quality: 0%, candidate: 0.0 BPM, periodic: 0.0 BPM/0%, AC RMS: 0.0, threshold: 0.0, crossings: 0, intervals: 0/0, diagnostic: filling window (11%).
Collecting a stable signal. IR: 43977, red: 47526, perfusion: 0.00%, red perfusion: 0.00%, correlation: 0.00, ratio: 0.00, quality: 0%, candidate: 0.0 BPM, periodic: 0.0 BPM/0%, AC RMS: 0.0, threshold: 0.0, crossings: 0, intervals: 0/0, diagnostic: filling window (24%).
Collecting a stable signal. IR: 44475, red: 47686, perfusion: 0.00%, red perfusion: 0.00%, correlation: 0.00, ratio: 0.00, quality: 0%, candidate: 0.0 BPM, periodic: 0.0 BPM/0%, AC RMS: 0.0, threshold: 0.0, crossings: 0, intervals: 0/0, diagnostic: filling window (36%).
Collecting a stable signal. IR: 44391, red: 47465, perfusion: 0.00%, red perfusion: 0.00%, correlation: 0.00, ratio: 0.00, quality: 0%, candidate: 0.0 BPM, periodic: 0.0 BPM/0%, AC RMS: 0.0, threshold: 0.0, crossings: 0, intervals: 0/0, diagnostic: filling window (49%).
Collecting a stable signal. IR: 43763, red: 46719, perfusion: 0.00%, red perfusion: 0.00%, correlation: 0.00, ratio: 0.00, quality: 0%, candidate: 0.0 BPM, periodic: 0.0 BPM/0%, AC RMS: 0.0, threshold: 0.0, crossings: 0, intervals: 0/0, diagnostic: filling window (61%).
Collecting a stable signal. IR: 43755, red: 46705, perfusion: 0.00%, red perfusion: 0.00%, correlation: 0.00, ratio: 0.00, quality: 0%, candidate: 0.0 BPM, periodic: 0.0 BPM/0%, AC RMS: 0.0, threshold: 0.0, crossings: 0, intervals: 0/0, diagnostic: filling window (74%).
Collecting a stable signal. IR: 43217, red: 46278, perfusion: 0.00%, red perfusion: 0.00%, correlation: 0.00, ratio: 0.00, quality: 0%, candidate: 0.0 BPM, periodic: 0.0 BPM/0%, AC RMS: 0.0, threshold: 0.0, crossings: 0, intervals: 0/0, diagnostic: filling window (86%).
Collecting a stable signal. IR: 43655, red: 46470, perfusion: 0.00%, red perfusion: 0.00%, correlation: 0.00, ratio: 0.00, quality: 0%, candidate: 0.0 BPM, periodic: 0.0 BPM/0%, AC RMS: 0.0, threshold: 0.0, crossings: 0, intervals: 0/0, diagnostic: filling window (99%).
Collecting a stable signal. IR: 43408, red: 46197, perfusion: 0.48%, red perfusion: 0.33%, correlation: 0.96, ratio: 0.69, quality: 59%, candidate: 94.3 BPM, periodic: 93.9 BPM/8%, AC RMS: 210.0, threshold: 21.0, crossings: 8, intervals: 6/5, diagnostic: waiting for candidate confirmation.
Collecting a stable signal. IR: 43593, red: 46250, perfusion: 0.48%, red perfusion: 0.33%, correlation: 0.96, ratio: 0.69, quality: 59%, candidate: 94.3 BPM, periodic: 93.9 BPM/8%, AC RMS: 210.0, threshold: 21.0, crossings: 8, intervals: 6/5, diagnostic: waiting for candidate confirmation.
Collecting a stable signal. IR: 43172, red: 45897, perfusion: 0.47%, red perfusion: 0.33%, correlation: 0.96, ratio: 0.71, quality: 24%, candidate: 94.3 BPM, periodic: 94.4 BPM/7%, AC RMS: 207.3, threshold: 20.7, crossings: 9, intervals: 6/3, diagnostic: beat intervals are inconsistent.
Collecting a stable signal. IR: 43470, red: 46011, perfusion: 0.47%, red perfusion: 0.33%, correlation: 0.96, ratio: 0.71, quality: 24%, candidate: 94.3 BPM, periodic: 94.4 BPM/7%, AC RMS: 207.3, threshold: 20.7, crossings: 9, intervals: 6/3, diagnostic: beat intervals are inconsistent.
Pulse: 92.1 BPM, SpO2: 92.8%, perfusion: 0.41%, red perfusion: 0.29%, correlation: 0.96, ratio: 0.69, periodic: 93.7 BPM/24%, quality: 53%.
Pulse: 92.1 BPM, SpO2: 92.8%, perfusion: 0.41%, red perfusion: 0.29%, correlation: 0.96, ratio: 0.69, periodic: 93.7 BPM/24%, quality: 53%.
Pulse: 92.3 BPM, SpO2: 93.4%, perfusion: 0.34%, red perfusion: 0.19%, correlation: 0.99, ratio: 0.56, periodic: 93.5 BPM/81%, quality: 80%.
Pulse: 92.3 BPM, SpO2: 93.4%, perfusion: 0.34%, red perfusion: 0.19%, correlation: 0.99, ratio: 0.56, periodic: 93.5 BPM/81%, quality: 80%.
Pulse: 92.5 BPM, SpO2: 93.9%, perfusion: 0.25%, red perfusion: 0.14%, correlation: 0.99, ratio: 0.57, periodic: 93.5 BPM/98%, quality: 94%.
Pulse: 92.5 BPM, SpO2: 93.9%, perfusion: 0.25%, red perfusion: 0.14%, correlation: 0.99, ratio: 0.57, periodic: 93.5 BPM/98%, quality: 94%.
Pulse: 92.7 BPM, SpO2: 94.3%, perfusion: 0.25%, red perfusion: 0.14%, correlation: 0.99, ratio: 0.57, periodic: 93.3 BPM/98%, quality: 95%.
Pulse: 92.7 BPM, SpO2: 94.3%, perfusion: 0.25%, red perfusion: 0.14%, correlation: 0.99, ratio: 0.57, periodic: 93.3 BPM/98%, quality: 95%.
Pulse: 92.9 BPM, SpO2: 94.5%, perfusion: 0.25%, red perfusion: 0.15%, correlation: 0.99, ratio: 0.58, periodic: 93.3 BPM/99%, quality: 96%.
Pulse: 92.9 BPM, SpO2: 94.5%, perfusion: 0.25%, red perfusion: 0.15%, correlation: 0.99, ratio: 0.58, periodic: 93.3 BPM/99%, quality: 96%.
```

## Oxygen-saturation calibration

The default conversion is:

```text
SpO₂ = 110 - 25 × RatioOfRatios
```

This is a generic approximation, not a factory calibration for a particular MAX30100 module. Optical cover material, sensor spacing, LED characteristics, current, pulse width, finger placement, skin properties, temperature, and mechanical pressure can all change the relationship.

`CalibrateOxygenSaturation` performs an ordinary least-squares fit of:

```text
reference SpO₂ = intercept + slope × RatioOfRatios
```

using paired ratio and reference measurements:

```csharp
double[] ratios = new double[]
{
    0.52,
    0.61,
    0.73,
    0.88,
    1.02
};

double[] referenceOxygenSaturations = new double[]
{
    99.0,
    97.0,
    94.0,
    90.0,
    86.0
};

pulseOximeter.CalibrateOxygenSaturation(ratios, referenceOxygenSaturations);

Debug.WriteLine(
    $"Calibration intercept: {pulseOximeter.OxygenSaturationCalibrationIntercept:F4}, "
    + $"slope: {pulseOximeter.OxygenSaturationCalibrationSlope:F4}");
```

The arrays are examples of the required shape only; they are not calibration data and must not be copied into an application.

For `Max30100PulseOximeterWorker`, calibration is applied asynchronously on the processing thread. Read the coefficient properties after the worker has processed the request, or store the coefficients printed after calibration and restore them at startup:

```csharp
pulseOximeter.SetOxygenSaturationCalibration(112.35, -27.42);
```

Use this procedure to collect calibration data:

1. Keep the exact production configuration unchanged: sensor module, optical cover, mounting, sample rate, pulse width, LED currents, and analysis settings.
2. Use a calibrated optical simulator or collect measurements under qualified clinical supervision with a suitable reference instrument. Never intentionally reduce a person's oxygen level to create calibration points.
3. Record reference SpO₂ and `RatioOfRatios` simultaneously. Do not pair readings taken at different times.
4. Accept a point only after the analysis window is full and readings are stable. As a starting criterion, require valid heart rate, signal quality of at least 80%, channel correlation of at least 0.90, and at least 0.10% perfusion in both channels.
5. Average several consecutive, independent analysis results at each reference level. Do not treat duplicated sample output between processor updates as separate observations.
6. Collect at least five distinct reference levels distributed across the intended measurement range. Many measurements confined to 95–100% do not provide enough ratio spread to determine a reliable slope.
7. Remove points affected by motion, clipping, FIFO loss, changing pressure, or failed validity checks before fitting.
8. Fit the coefficients with `CalibrateOxygenSaturation`, then save the reported intercept and slope in application configuration.
9. Validate the saved coefficients on separate measurements that were not used for fitting. Report error across the range, not only at one convenient saturation level.
10. Repeat calibration whenever the module, enclosure, optical path, LED settings, or mounting changes.

The fitted slope must be negative, the ratios must not all be equal, and at least two paired points are required. In practice, two-point calibration is highly sensitive to noise and is not recommended. Least-squares fitting reduces random error but does not make the MAX30100 implementation medically validated.

The MAX30100 does not directly return beats per minute or oxygen saturation. The estimates from this binding are intended for experimentation and demonstration only. They are not calibrated, validated, or suitable for medical diagnosis.
