(function (global) {
    global.CompactJS = global.CompactJS || {};
    CompactJS.Utilities = CompactJS.Utilities || {};
})(this);

(function applicationInit(global, CompactJS, undefined) {

    var _clickCount = 0;
    var _countButton;

    var addButtonClickHandler = function() {
        _countButton.add_Click(function() { 
            _clickCount++; 
            _countButton.Text = _clickCount;
            changeAppTitle(_clickCount); 
        });
        
        if (_countButton.Text == 'enable first') 
            _countButton.Text = 'press me';
            
        changeAppTitle('Added Button Click'); 
    }

    var removeButtonClickHandler = function() {
        clearEventInvocations(_countButton, 'Click');
        
        changeAppTitle('RemovedButton Click'); 
    }

    var changeAppTitle = function(title) {
        mainForm.Text = title;
    }

    var buildBasicGui = function() {
        _countButton = new System.Windows.Forms.Button();
        _countButton.Text = 'enable first';
        _countButton.Location = new System.Drawing.Point(3, 3);
        _countButton.Size = new System.Drawing.Size(100, 20);
        mainForm.Controls.Add(_countButton);

        var addHandlerButton = new System.Windows.Forms.Button();
        addHandlerButton.Text = 'Enable counter';
        addHandlerButton.Location = new System.Drawing.Point(113, 3);
        addHandlerButton.Size = new System.Drawing.Size(120, 20);
        addHandlerButton.add_Click(addButtonClickHandler);
        mainForm.Controls.Add(addHandlerButton);
        
        var removeHandlerButton = new System.Windows.Forms.Button();
        removeHandlerButton.Text = 'Disable counter';
        removeHandlerButton.Location = new System.Drawing.Point(113, 25);
        removeHandlerButton.Size = new System.Drawing.Size(120, 20);
        removeHandlerButton.add_Click(removeButtonClickHandler);
        mainForm.Controls.Add(removeHandlerButton);

        
        var label = new System.Windows.Forms.Label();
        label.Location = new System.Drawing.Point(3, 180);
        label.Size = new System.Drawing.Size(200, 20);
        label.Text = 'KeyUp and Press events:'
        mainForm.Controls.Add(label);
        
        var textbox = new System.Windows.Forms.TextBox();
        textbox.Location = new System.Drawing.Point(3, 200);
        textbox.Size = new System.Drawing.Size(200, 20);
        textbox.add_KeyUp(function(s,e) {
            mainForm.Text = e.KeyValue;
        });
        textbox.add_KeyPress(function(s, e) {
            if (e.KeyChar == 'i') e.Handled = true; // Do not allow the 'i' character in the TextBox to show up. KeyUp still registers this key.
        });
        mainForm.Controls.Add(textbox);
    }


    CompactJS.Namespace.define("CompactJS.Application", {
        
        addButtonClickHandler: function () {
            addButtonClickHandler();
        },
        
        removeButtonClickHandler: function () {
            removeButtonClickHandler();
        },
        
        changeAppTitle: function (title) {
            changeAppTitle(title);
        },
        
    });

    changeAppTitle('Test Title');
    buildBasicGui();

})(this, CompactJS);
