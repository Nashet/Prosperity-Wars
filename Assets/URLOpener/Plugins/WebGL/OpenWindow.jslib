var OpenWindowPlugin = {  
    OpenWindow: function(link)
    {
        var url = Pointer_stringify(link);
        window.open(url,'_blank');
    }
};

mergeInto(LibraryManager.library, OpenWindowPlugin); 