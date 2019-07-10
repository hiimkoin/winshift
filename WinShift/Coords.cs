using System.Drawing;
using System.Windows.Forms;

namespace WinShift {
    abstract class Coords {

        // TODO shouldnt be hardcoded
        public int x = Screen.AllScreens[1].WorkingArea.X;
        public int y = Screen.AllScreens[1].WorkingArea.Y;
        public int width = Screen.AllScreens[1].WorkingArea.Width;
        public int height = Screen.AllScreens[1].WorkingArea.Height;

        public abstract Rectangle coords();

    }

    class TopCoords : Coords {
        public override Rectangle coords() {

            var newHeight = height / 2;

            return new Rectangle(
                x, y, width, newHeight
            );
        }
    }

    class DownCoords : Coords {
        public override Rectangle coords() {

            var newHeight = height / 2;
            var newY = y + newHeight;

            return new Rectangle(
                x, newY, width, newHeight
            );
        }
    }

    class CenterCoords : Coords {
        public override Rectangle coords() {
            // Doesnt really Center it sets position same height as my left landscape display
            return new Rectangle(
                x, 0, width, 1080
            );
        }
    }

    class FirstThird : Coords {
        public override Rectangle coords() {

            var newHeight = height / 3;
            
            return new Rectangle(
                x, y, width, newHeight
            );
        }
    }

    class SecondThird : Coords {
        public override Rectangle coords() {

            var newHeight = height / 3;
            var newY = y + newHeight;

            return new Rectangle(
                x, newY, width, newHeight
            );
        }
    }

    class ThirdThird : Coords {
        public override Rectangle coords() {

            var newHeight = height / 3;
            var newY = y + (newHeight * 2);

            return new Rectangle(
                x, newY, width, newHeight
            );
        }
    }

    class FirstSecondThird : Coords {
        public override Rectangle coords() {

            var newHeight = height / 3;
            
            return new Rectangle(
                x, y, width, newHeight
            );
        }
    }

    class SecondSecondThird : Coords {
        public override Rectangle coords() {

            var newY = y + (height) / 3;
            var newHeight = (height * 2) / 3;

            return new Rectangle(
                x, newY, width, newHeight
            );
        }
    }
}
