using MissionPlanner.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionPlanner.Swarm
{
    public class DigitalLPF
    {
        public DigitalLPF()
        {
        }
        // add a new raw value to the filter, retrieve the filtered result
        public float apply(float sample, float cutoff_freq, float dt)
        {
            if (cutoff_freq <= 0.0f || dt <= 0.0f)
            {
                _output = sample;
                return _output;
            }
            float rc = 1.0f / (float)(2*Math.PI * cutoff_freq);
            alpha = (float)MathHelper.constrain(dt / (dt + rc), 0.0f, 1.0f);
            _output += (sample - _output) * alpha;
            return _output;
        }
        public float apply(float sample)
        {
            _output += (sample - _output) * alpha;
            return _output;
        }

        public void compute_alpha(float sample_freq, float cutoff_freq)
        {
            if (cutoff_freq <= 0.0f || sample_freq <= 0.0f)
            {
                alpha = 1.0f;
            }
            else
            {
                float dt = 1.0f / sample_freq;
                float rc = 1.0f / (float)(2 * Math.PI * cutoff_freq);
                alpha = (float)MathHelper.constrain(dt / (dt + rc), 0.0f, 1.0f);
            }
        }

        // get latest filtered value from filter (equal to the value returned by latest call to apply method)
        public float get()
        {
            return _output;
        }
        public void reset(float value)
        {
            _output = value;
        }
        float _output;
        float alpha = 1.0f;
    }
    public class LowPassFilter
    {
        public LowPassFilter()
        {
            _cutoff_freq = 0.0f;
        }
        public LowPassFilter(float cutoff_freq)
        {
            _cutoff_freq = cutoff_freq;
        }
        public LowPassFilter(float sample_freq, float cutoff_freq)
        {
            set_cutoff_frequency(sample_freq, cutoff_freq);
        }

        // change parameters
        public void set_cutoff_frequency(float cutoff_freq)
        {
            _cutoff_freq = cutoff_freq;
        }
        public void set_cutoff_frequency(float sample_freq, float cutoff_freq)
        {
            _cutoff_freq = cutoff_freq;
            _filter.compute_alpha(sample_freq, cutoff_freq);
        }

        // return the cutoff frequency
        public float get_cutoff_freq()
        {
            return _cutoff_freq;
        }
        public float apply(float sample, float dt)
        {
            return _filter.apply(sample, _cutoff_freq, dt);
        }
        public float apply(float sample)
        {
            return _filter.apply(sample);
        }
        public float get()
        {
            return _filter.get();
        }
        public void reset(float value)
        {
            _filter.reset(value);
        }
        public void reset() { reset(0.0f); }

        protected float _cutoff_freq;

        DigitalLPF _filter =new DigitalLPF();
    }
    public class DigitalBiquadFilter
    {
        public struct biquad_params
        {
            public float cutoff_freq;
            public float sample_freq;
            public float a1;
            public float a2;
            public float b0;
            public float b1;
            public float b2;
        };

        public DigitalBiquadFilter()
        {

        }

        public float apply(float sample, biquad_params param)
        {
            if (param.cutoff_freq.Equals(0) || param.sample_freq.Equals(0))
            {
                return sample;
            }

            float delay_element_0 = sample - _delay_element_1 * param.a1 - _delay_element_2 * param.a2;
            float output = delay_element_0 * param.b0 + _delay_element_1 * param.b1 + _delay_element_2 * param.b2;

            _delay_element_2 = _delay_element_1;
            _delay_element_1 = delay_element_0;

            return output;
        }
        public void reset()
        {
            _delay_element_1 = _delay_element_2 = 0.0f;
        }
        public static void compute_params(float sample_freq, float cutoff_freq, ref biquad_params ret)
        {
            ret.cutoff_freq = cutoff_freq;
            ret.sample_freq = sample_freq;

            float fr = sample_freq / cutoff_freq;
            float ohm = (float)Math.Tan(Math.PI / fr);
            float c = 1.0f + 2.0f * (float)Math.Cos(Math.PI / 4.0f) * ohm + ohm * ohm;

            ret.b0 = ohm * ohm / c;
            ret.b1 = 2.0f * ret.b0;
            ret.b2 = ret.b0;
            ret.a1 = 2.0f * (ohm * ohm - 1.0f) / c;
            ret.a2 = (1.0f - 2.0f * (float)Math.Cos(Math.PI / 4.0f) * ohm + ohm * ohm) / c;
        }

        float _delay_element_1;
        float _delay_element_2;
    }
    public class LowPassFilter2p
    {
        public LowPassFilter2p()
        {
            _params.cutoff_freq = _params.sample_freq = _params.a1 = _params.a2 = _params.b0 = _params.b1 = _params.b2 = 0.0f;
        }
        // constructor
        public LowPassFilter2p(float sample_freq, float cutoff_freq)
        {
            set_cutoff_frequency(sample_freq, cutoff_freq);
        }
        // change parameters
        public void set_cutoff_frequency(float sample_freq, float cutoff_freq)
        {
            DigitalBiquadFilter.compute_params(sample_freq, cutoff_freq, ref _params);
        }
        // return the cutoff frequency
        public float get_cutoff_freq()
        {
            return _params.cutoff_freq;
        }
        public float get_sample_freq()
        {
            return _params.sample_freq;
        }
        public float apply(float sample)
        {
            return _filter.apply(sample, _params);
        }
        public void reset()
        {
            _filter.reset();
        }

        protected  DigitalBiquadFilter.biquad_params _params = new DigitalBiquadFilter.biquad_params();
        DigitalBiquadFilter _filter = new DigitalBiquadFilter();
    }
}
