using PhoVodKEdit.BasicEffects.ABS;
using PhoVodKEdit.Port.APS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PhoVodKEdit.BasicEffects {
    internal class FeatureDetection : MascingPortEffect {
		protected int lowestBorder = 5000;
		protected int maximumBorder = 25000;
		protected int lastMaximum = 0;
		protected bool setLastMaximumAsMaximumBorder = true;

		public FeatureDetection(AppliedSettings _applied) : base(_applied) {
			Offset = 0;
			Factor = 1;
			IsThreadCountMultiplyerFixed = true;
			ThreadCountMultiplyer = 1;
		}

        public override FrameworkElement GetView() {
            Grid grid = new Grid();
			//grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			//grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());

			Label lbLowerstBorder = new Label() {
				Content = "Filter Less"
			};
			Label lbMaximumBorder = new Label() {
				Content = "Cut low at"
			};
			Label lbLastMaximum = new Label() {
				Content = "Last max value:"
			};
			Label lbEnableLastMaximumSet = new Label() {
				Content = "Set last max value for 'Cutting' "
			};

			TextBox tbLowestBorder = new TextBox() {
				Text = lowestBorder.ToString()
            };
			TextBox tbMaximumBorder = new TextBox() {
				Text = maximumBorder.ToString()
			};
			Label lbLastMaxValue = new Label() {
				Content = lastMaximum.ToString()
			};
			CheckBox cbEnableLastMaximumSet = new CheckBox() {
				IsChecked = setLastMaximumAsMaximumBorder,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};
			Button btnSave = new Button() {
				Content = "Set"
			};

			cbEnableLastMaximumSet.Click += (s, e) => {
				bool? ischecked = (s as CheckBox).IsChecked;
				setLastMaximumAsMaximumBorder = ischecked == null ? false : (bool)ischecked;
			};
			btnSave.Click += (s, e) => {
				int.TryParse(tbLowestBorder.Text, out lowestBorder);
				int.TryParse(tbMaximumBorder.Text, out maximumBorder);
				cbEnableLastMaximumSet.IsChecked = setLastMaximumAsMaximumBorder;
				(MainWindow as MainWindow).ApplyEffects();
			};

			Grid.SetColumn(lbMaximumBorder, 2);
			Grid.SetColumn(lbLastMaxValue, 1);
			Grid.SetColumn(lbEnableLastMaximumSet, 1);
			Grid.SetColumn(tbLowestBorder, 1);
			Grid.SetColumn(tbMaximumBorder, 3);
			Grid.SetColumn(btnSave, 3);

			Grid.SetColumnSpan(lbLastMaximum, 2);
			Grid.SetColumnSpan(lbEnableLastMaximumSet, 3);

			Grid.SetRow(lbLastMaximum, 1);
			Grid.SetRow(lbLastMaxValue, 1);
			Grid.SetRow(lbEnableLastMaximumSet, 2);
			Grid.SetRow(cbEnableLastMaximumSet, 2);
			Grid.SetRow(btnSave, 2);

			grid.Children.Add(lbLowerstBorder);
			grid.Children.Add(lbMaximumBorder);
			grid.Children.Add(lbLastMaximum);
			grid.Children.Add(lbEnableLastMaximumSet);
			grid.Children.Add(tbLowestBorder);
			grid.Children.Add(tbMaximumBorder);
			grid.Children.Add(lbLastMaxValue);
			grid.Children.Add(cbEnableLastMaximumSet);
			grid.Children.Add(btnSave);

			return grid;
        }

        protected override unsafe void Implement(Bitmap image, PixelFormat pixelFormat, BitmapData bitmapData, int stride, IntPtr Scan0, IntPtr originalScan0) {
			int nextPixel = pixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;

			Bitmap x, y;
			CopyImage(image, out x, bitmapData);
			CopyImage(image, out y, bitmapData);
			var bmdX = x.LockBits(new Rectangle(0, 0, x.Width, x.Height), ImageLockMode.ReadWrite, pixelFormat);
			var bmdY = y.LockBits(new Rectangle(0, 0, y.Width, y.Height), ImageLockMode.ReadWrite, pixelFormat);

			// grads
			Implement(x, pixelFormat, bmdX, stride, bmdX.Scan0, originalScan0, new double[] {
				-1, 0, 1,
				-1, 0, 1,
				-1, 0, 1
			}, null);
			Implement(y, pixelFormat, bmdY, stride, bmdY.Scan0, originalScan0, new double[] {
				-1, -1, -1,
				 0,  0,  0,
				 1,  1,  1
            }, null);

			int picWidth = stride * image.Height;
			int[] xy = new int[picWidth];
			int[] xpow = new int[picWidth];
			int[] ypow = new int[picWidth];

			byte* px = (byte*)(void*)bmdX.Scan0;
			byte* py = (byte*)(void*)bmdY.Scan0;

			if (!IsThreadCountMultiplyerFixed) {
				ThreadCountMultiplyer = (int)Math.Sqrt(picWidth / 518400); // number assures that there is 4 threads on 1080p
			}

			//int isum = nextPixel - 2;
			fixed (int* fxy = xy, fxpow = xpow, fypow = ypow) {
				int* pxy = fxy;
				int* pxpow = fxpow;
				int* pypow = fypow;

				CalculatePows(px, py, pxy, pxpow, pypow, picWidth, nextPixel);
				/*
				#region Calculate Pows
				if (ThreadCountMultiplyer == 0) {
					CalculatePows(px, py, pxy, pxpow, pypow, picWidth, nextPixel);
				}
				else {
					picWidth >>= ThreadCountMultiplyer;
					Task[] tasks = new Task[(int)Math.Pow(2, ThreadCountMultiplyer)];
					{
						byte* spx = px;
						byte* spy = py;
						int* spxy = pxy;
						int* spxpow = pxpow;
						int* spypow = pypow;

						tasks[0] = new Task(() => CalculatePows(spx, spy, spxy, spxpow, spypow, picWidth, nextPixel));
					}

                    //tasks[0].Start();
                    //tasks[0].Wait();

                    for (int i = 1; i < tasks.Length; i++) {
                        px += picWidth;
						py += picWidth;
						pxy += picWidth;
						pxpow += picWidth;
						pypow += picWidth;

                        {
                            byte* spx = px;
                            byte* spy = py;
                            int* spxy = pxy;
                            int* spxpow = pxpow;
                            int* spypow = pypow;

                            tasks[i] = new Task(() => CalculatePows(spx, spy, spxy, spxpow, spypow, picWidth, nextPixel));
                        }
                    }

                    for (int i = 0; i < tasks.Length; i++) {
                        tasks[i].Start();
                    }

                    Task.WaitAll(tasks);
                }
				#endregion Calculate Pows
				*/
			}

			int cWidth = picWidth - (stride << 1) - (nextPixel << 1);
			int[] lxsum = new int[cWidth];
			int[] lysum = new int[cWidth];
			int[] xysum = new int[cWidth];
			int[] filteredPoints = new int[cWidth];

			byte* p = (byte*)(void*)Scan0 + stride + nextPixel;

			fixed (int* fxul = xpow, fyul = ypow, fxyul = xy, flxsum = lxsum, flysum = lysum, fxysum = xysum, ffp = filteredPoints) {
				int* pxul = fxul;
				int* pxml = pxul + stride;
				int* pxbl = pxml + stride;

				int* pyul = fyul;
				int* pyml = pyul + stride;
				int* pybl = pyml + stride;

				int* pxyul = fxyul;
				int* pxyml = pxyul + stride;
				int* pxybl = pxyml + stride;

				int* plxsum = flxsum;
				int* plysum = flysum;
				int* pxysum = fxysum;
				int* pfp = ffp;

				int nextPixel2 = nextPixel << 1;
				double upperFilter = maximumBorder / 255;

                // For debug reasons...
                //SortedDictionary<int, int> dbsz = new SortedDictionary<int, int>();
				const int i0 = 0;
				const int i1 = 1;
				const int i2 = 2;

				int i0Next = i0 + nextPixel;
				int i0Next2 = i0 + nextPixel2;
				int i1Next = i1 + nextPixel;
				int i1Next2 = i1 + nextPixel2;
				int i2Next = i2 + nextPixel;
				int i2Next2 = i0 + nextPixel2;

				for (int i = 0; i < cWidth / nextPixel; i++) {
					

					plxsum[i0] = pxul[i0] + pxul[i0Next] + pxul[i0Next2]
							   + pxml[i0] + pxml[i0Next] + pxml[i0Next2]
							   + pxbl[i0] + pxbl[i0Next] + pxbl[i0Next2];
					plxsum[i1] = pxul[i1] + pxul[i1Next] + pxul[i1Next2]
							   + pxml[i1] + pxml[i1Next] + pxml[i1Next2]
							   + pxbl[i1] + pxbl[i1Next] + pxbl[i1Next2];
					plxsum[i2] = pxul[i2] + pxul[i2Next] + pxul[i2Next2]
							   + pxml[i2] + pxml[i2Next] + pxml[i2Next2]
							   + pxbl[i2] + pxbl[i2Next] + pxbl[i2Next2];

					plysum[i0] = pyul[i0] + pyul[i0Next] + pyul[i0Next2]
							   + pyml[i0] + pyml[i0Next] + pyml[i0Next2]
							   + pybl[i0] + pybl[i0Next] + pybl[i0Next2];
					plysum[i1] = pyul[i1] + pyul[i1Next] + pyul[i1Next2]
							   + pyml[i1] + pyml[i1Next] + pyml[i1Next2]
							   + pybl[i1] + pybl[i1Next] + pybl[i1Next2];
					plysum[i2] = pyul[i2] + pyul[i2Next] + pyul[i2Next2]
							   + pyml[i2] + pyml[i2Next] + pyml[i2Next2]
							   + pybl[i2] + pybl[i2Next] + pybl[i2Next2];

					pxysum[i0] = pxyul[i0] + pxyul[i0Next] + pxyul[i0Next2]
							   + pxyml[i0] + pxyml[i0Next] + pxyml[i0Next2]
							   + pxybl[i0] + pxybl[i0Next] + pxybl[i0Next2];
					pxysum[i1] = pxyul[i1] + pxyul[i1Next] + pxyul[i1Next2]
							   + pxyml[i1] + pxyml[i1Next] + pxyml[i1Next2]
							   + pxybl[i1] + pxybl[i1Next] + pxybl[i1Next2];
					pxysum[i2] = pxyul[i2] + pxyul[i2Next] + pxyul[i2Next2]
							   + pxyml[i2] + pxyml[i2Next] + pxyml[i2Next2]
							   + pxybl[i2] + pxybl[i2Next] + pxybl[i2Next2];

					if (plxsum[i0] != 0 && plysum[i0] != 0) {
						pfp[i0] = (plxsum[i0] * plysum[i0] - (pxysum[i0] << 1)) / (plxsum[i0] + plysum[i0]);
						if (setLastMaximumAsMaximumBorder && pfp[i0] > lastMaximum) lastMaximum = pfp[i0]; 
                        if (pfp[i0] < lowestBorder) pfp[i0] = 0;
                        else if (pfp[i0] > maximumBorder) pfp[i0] = maximumBorder;
                    }
					if (plxsum[i1] != 0 && plysum[i1] != 0) {
						pfp[i1] = (plxsum[i1] * plysum[i1] - (pxysum[i1] << 1)) / (plxsum[i1] + plysum[i1]);
						if (setLastMaximumAsMaximumBorder && pfp[i1] > lastMaximum) lastMaximum = pfp[i1];
						if (pfp[i1] < lowestBorder) pfp[i1] = 0;
                        else if (pfp[i1] > maximumBorder) pfp[i1] = maximumBorder;
                    }
					if (plxsum[i2] != 0 && plysum[i2] != 0) {
						pfp[i2] = (plxsum[i2] * plysum[i2] - (pxysum[i2] << 1)) / (plxsum[i2] + plysum[i2]);
						if (setLastMaximumAsMaximumBorder && pfp[i2] > lastMaximum) lastMaximum = pfp[i2];
						if (pfp[i2] < lowestBorder) pfp[i2] = 0;
                        else if (pfp[i2] > maximumBorder) pfp[i2] = maximumBorder;
                    }

					p[i0] = (byte)((double)pfp[i0] / upperFilter);
					p[i1] = (byte)((double)pfp[i1] / upperFilter);
					p[i2] = (byte)((double)pfp[i2] / upperFilter);


					plxsum += nextPixel;
					plysum += nextPixel;
					pxysum += nextPixel;

					pxul += nextPixel;
					pxml += nextPixel;
					pxbl += nextPixel;

					pyul += nextPixel;
					pyml += nextPixel;
					pybl += nextPixel;

					pxyul += nextPixel;
					pxyml += nextPixel;
					pxybl += nextPixel;

					pfp += nextPixel;
					p += nextPixel;

                    //if (!dbsz.ContainsKey(filteredPoints[i0])) dbsz.Add(filteredPoints[i0], 1);
                    //else dbsz[filteredPoints[i0]]++;
                    //if (!dbsz.ContainsKey(filteredPoints[i1])) dbsz.Add(filteredPoints[i1], 1);
                    //else dbsz[filteredPoints[i1]]++;
                    //if (!dbsz.ContainsKey(filteredPoints[i2])) dbsz.Add(filteredPoints[i2], 1);
                    //else dbsz[filteredPoints[i2]]++;
                }

			}

			x.UnlockBits(bmdX);
			y.UnlockBits(bmdY);

		}

		protected unsafe void CalculatePows(byte* px, byte* py, int* pxy, int* pxpow, int* pypow, int picWidth, int nextPixel) {
			for (int i = 0; i < picWidth / nextPixel; i++) {
				//int i0 = i;
				//int i1 = ++i;
				//int i2 = ++i;

				//xy[i0] = px[i0] * py[i0];
				//xy[i1] = px[i1] * py[i1];
				//xy[i2] = px[i2] * py[i2];

				//xpow[i0] = px[i0] * px[i0];
				//xpow[i1] = px[i1] * px[i1];
				//xpow[i2] = px[i2] * px[i2];

				//ypow[i0] = py[i0] * py[i0];
				//ypow[i1] = py[i1] * py[i1];
				//ypow[i2] = py[i2] * py[i2];

				pxy[0] = px[0] * py[0];
				pxy[1] = px[1] * py[1];
				pxy[2] = px[2] * py[2];

				pxpow[0] = px[0] * px[0];
				pxpow[1] = px[1] * px[1];
				pxpow[2] = px[2] * px[2];

				pypow[0] = py[0] * py[0];
				pypow[1] = py[1] * py[1];
				pypow[2] = py[2] * py[2];

				px += nextPixel;
				py += nextPixel;
				pxy += nextPixel;
				pxpow += nextPixel;
				pypow += nextPixel;
			}
		}

		protected unsafe void CalculateEdges() {

        }

		protected override double[] CalculateMask() {
			return new double[] {
				1, 1, 1,
				1, 1, 1,
				1, 1, 1
			};
        }
    }
}
