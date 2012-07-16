#pragma once

using namespace System;
using namespace AudioClasses;

namespace ImageAquisition 
{

	public delegate void DelegateSong(String ^strSong, Object ^objSender);

	public ref class AudioFileReader : AudioClasses::IAudioSource, System::ComponentModel::INotifyPropertyChanged
	{
	public:
			AudioFileReader(AudioClasses::AudioFormat ^audioformat);
			~AudioFileReader();

			event DelegateSong ^OnPlayStarted;
			event DelegateSong ^OnPlayFinished;

			PlayList ^OurPlayList;
		
			virtual AudioClasses::MediaSample ^PullSample(AudioClasses::AudioFormat ^format, TimeSpan tsDuration);
	        property bool IsSourceActive
			{
				virtual bool get()
				{
					return m_bActive;
				}
				virtual void set(bool bValue)
				{
					m_bActive = bValue;
					StartNextFileQueue();
					FirePropertyChanged("IsSourceActive");
				}
			}

			property bool IsPlaying
			{
				bool get()
				{
					return m_bIsPlaying;
				}
				void set(bool bValue)
				{
					if (m_bIsPlaying != bValue)
					{
						m_bIsPlaying = bValue;
						FirePropertyChanged("IsPlaying");
					}
				
				}
			}

		    property double SourceAmplitudeMultiplier 
			{ 
				virtual double get()
				{
					return m_fAmplitude;
				}
				virtual void set(double fValue)
				{
					m_fAmplitude = fValue;
				}
			}

			
			property double PlayProgressPercent 
			{ 
				double get()
				{
					if (m_nBytesInCurrentSong <= 0)
						return 0.0f;

					/// We don't know the final length so this may not be super accurate at start time until we resample the song
					return ((double) (m_nBytesInCurrentSong-EnqueuedAudioData->Size)/m_nBytesInCurrentSong)*100.0f;
				}
			}

			virtual event System::ComponentModel::PropertyChangedEventHandler ^PropertyChanged;

		protected:


			void AbortCurrentSong();
			

			Track ^CurrentTrack;

			void OnPlaylistChanged(System::Object ^obj, System::ComponentModel::PropertyChangedEventArgs ^args);
			
			void FirePlayStarted(String ^strSong)
			{
				OnPlayStarted(strSong, this);
			}
			void FirePlayFinished(String ^strSong)
			{
				OnPlayFinished(strSong, this);
			}


			void FirePropertyChanged(System::String ^strProp)
			{
  			   PropertyChanged(this, gcnew System::ComponentModel::PropertyChangedEventArgs(strProp));
			}

			AudioClasses::AudioFormat ^OutputAudioFormat;

			void StartNextFileQueue();
			static void QueueFileThread(Object ^objFileName);
			void FinishCurrentSong();

			AudioClasses::ByteBuffer ^EnqueuedAudioData;
			
		
			System::Threading::ManualResetEvent ^ThreadFinishedEvent;

			double m_fAmplitude;
			bool m_bActive;
			bool m_bIsPlaying;
			bool m_bAbortRead;
			bool m_bFileProcessing;
			int m_nBytesInCurrentSong;
	};



}