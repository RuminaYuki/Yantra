using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using TMPro;
public class TextSwitcherMixerBehaviour : PlayableBehaviour
{
    Color m_DefaultColor;
    float m_DefaultFontSize; // เปลี่ยนเป็น float ให้ตรงกับ TextMeshPro
    string m_DefaultText;

    TextMeshProUGUI m_TrackBinding; // เปลี่ยนชนิดตัวแปรจาก Text เป็น TextMeshProUGUI
    bool m_FirstFrameHappened;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // รับค่า binding เป็น TextMeshProUGUI
        m_TrackBinding = playerData as TextMeshProUGUI;

        if (m_TrackBinding == null)
            return;

        if (!m_FirstFrameHappened)
        {
            m_DefaultColor = m_TrackBinding.color;
            m_DefaultFontSize = m_TrackBinding.fontSize;
            m_DefaultText = m_TrackBinding.text;
            m_FirstFrameHappened = true;
        }

        int inputCount = playable.GetInputCount();

        Color blendedColor = Color.clear;
        float blendedFontSize = 0f;
        float totalWeight = 0f;
        float greatestWeight = 0f;
        int currentInputs = 0;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<TextSwitcherBehaviour> inputPlayable = (ScriptPlayable<TextSwitcherBehaviour>)playable.GetInput(i);
            TextSwitcherBehaviour input = inputPlayable.GetBehaviour();

            blendedColor += input.color * inputWeight;
            blendedFontSize += input.fontSize * inputWeight;
            totalWeight += inputWeight;

            if (inputWeight > greatestWeight)
            {
                m_TrackBinding.text = input.text;
                greatestWeight = inputWeight;
            }

            if (!Mathf.Approximately(inputWeight, 0f))
                currentInputs++;
        }

        m_TrackBinding.color = blendedColor + m_DefaultColor * (1f - totalWeight);
        m_TrackBinding.fontSize = blendedFontSize + m_DefaultFontSize * (1f - totalWeight); // ใช้ float โดยไม่ต้อง RoundToInt

        if (currentInputs != 1 && 1f - totalWeight > greatestWeight)
        {
            m_TrackBinding.text = m_DefaultText;
        }
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        m_FirstFrameHappened = false;

        if (m_TrackBinding == null)
            return;

        m_TrackBinding.color = m_DefaultColor;
        m_TrackBinding.fontSize = m_DefaultFontSize;
        m_TrackBinding.text = m_DefaultText;
    }
}
