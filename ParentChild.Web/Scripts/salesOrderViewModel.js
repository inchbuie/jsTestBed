﻿
var ObjectState = {
    Unchanged: 0,
    Added: 1,
    Modified: 2,
    Deleted: 3
}

//tell knockout how parent & child relate
var salesOrderItemMapping = {
    "Items": {
        key: function (item) {
            //unwrapObservable: if item is observable return value() else return value
            return ko.utils.unwrapObservable(item.Id)
        },
        create: function (options) {
            // tell KO what to do for each SalesOrderItem it needs to create
            // use the data passed to the mapping definition
            // options arg is passed to callback (create() function)
            return new SalesOrderItemViewModel(options.data);
        }
    }
}

// knockout needs to send RowVersion (byte[]) as base-64 
// encoded value so that the server will recognize it
var dataConverter = function (key, value) {
    if (key === "RowVersion" && Array.isArray(value)) {
        var str = String.fromCharCode.apply(null, value);
        return btoa(str);
    }
    return value;
}

SalesOrderItemViewModel = function (data) {
    var self = this;
    ko.mapping.fromJS(data, salesOrderItemMapping, self);


    self.flagSalesOrderItemAsEdited = function () {
        if (self.ObjectState() != ObjectState.Added) {
            self.ObjectState(ObjectState.Modified);
            //console.log("modified!");
        }
        return true;
    },
    self.ExtendedPrice = ko.computed(function () {
        return (self.Quantity() * self.UnitPrice()).toFixed(2);
    })

}

SalesOrderViewModel = function (data) {
    var self = this;
    ko.mapping.fromJS(data, salesOrderItemMapping, self);

    self.save = function () {
        $.ajax({
            url: "/Sales/Save/",
            type: "POST",
            data: ko.toJSON(self, dataConverter),
            contentType: "application/json",
            success: function (data) {
                //if view model is returned directly
                //ko.mapping.fromJS(data, {}, self); 

                //return from save is an anonymous json object w/ viewmodel beneath
                if (data.salesOrderViewModel) {
                    ko.mapping.fromJS(data.salesOrderViewModel, {}, self);
                }

                if (data.newLocation != null) {
                    //redirect to new location
                    window.location = data.newLocation;
                }
            },
            error: function(xhr, status, error){

                if (xhr.status == 400) {
                    $("#MessageToClient").text(xhr.responseText);
                } else {
                    var err = eval("(" + xhr.responseText + ")");
                    if (console && console.log) console.log(err.Message);
                    $("#MessageToClient").text("The web server had an error (" + xhr.status + ")");
                }
            }
        });
    },

    self.flagAsEdited = function () {
        if (self.ObjectState() != ObjectState.Added) {
            self.ObjectState(ObjectState.Modified);
            //console.log("modified!");
        }
        return true;
    },

    self.addSalesOrderItem = function () {
        var item = new SalesOrderItemViewModel({
            Id: 0,
            ProductCode: "",
            Quantity: 1,
            UnitPrice: 0,
            SalesOrderId: self.Id,
            ObjectState: ObjectState.Added
        });
        self.Items.push(item);
    },

    self.Total = ko.computed(function () {
        var total = 0;
        ko.utils.arrayForEach(self.Items(), function(salesOrderItem){
            total += parseFloat(salesOrderItem.ExtendedPrice());
        })
        return total.toFixed(2);
    }),

    self.deleteSalesOrderItem = function (item) {
        self.Items.remove(this);

        //add removed item to list of deleted
        if (item.Id() > 0 && self.ItemsToDelete.indexOf(item.Id()) == -1) {
            self.ItemsToDelete.push(item.Id());
        }
    }
}

//jQuery validation
//intercept submit (validation successful)
$("form").validate({
    submitHandler: function () {
        //handle submit by calling save() on current instance of js view model
        salesOrderViewModel.save();
    },
    rules:{
        CustomerName: {
            required: true,
            maxlength: 30
        },
        PONumber: {
            maxlength: 10
        },
        ProductCode: {
            required: true,
            maxlength: 15,
            alphaonly: true
        },
        Quantity: {
            required: true,
            digits: true,
            range: [1, 1000000]
        },
        UnitPrice: {
            required: true,
            number: true,
            range: [0, 100000]
        }
    },
    messages: {
        //custom validation message
        CustomerName: {
            required: "You cannot create a sales order unless you specify the customer's name.",
            maxlength: "Customer names must be 30 characters or shorter."
        },
        ProductCode: {
            alphaonly: "Product codes consist of letters A-z only. "
        }
    },
    tooltip_options: {
        //customizing appearance of jquery-validate-bootstrap-tooltip
        CustomerName: {
            placement: "right"
        },
        PONumber: {
            placement: "right"
        }
    }

}).checkForm = function() {
    //override jQuery.validate checkForm() method!
    this.prepareForm();
    for (var i = 0, elements = (this.currentElements = this.elements()) ; elements[i]; i++) {

        //check name of current element (elements[i].name) to see how many elements have the same name
        //  for a child item that is a list, each item's element will have the same name
        //  unmodified jQuery.validate presumes a flat structure where each element's name is unique
        if (this.findByName(elements[i].name).length != undefined && this.findByName(elements[i].name).length > 1) {
            //iterate to find all the sub-elements with the same name
            for (var count = 0; count < this.findByName(elements[i].name).length; count++) {
                //check each one of these
                this.check(this.findByName(elements[i].name)[count]);
            }
        } else {
            this.check(elements[i]);
        }

    }
    return this.valid();
};

//custom validation method
$.validator.addMethod("alphaonly",
    function (value) {
        return /^[A-Za-z]+$/.test(value);
    }
);