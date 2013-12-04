function varargout = SCA_RBC(varargin)
%SCA_RBC M-file for SCA_RBC.fig
%      SCA_RBC, by itself, creates a new SCA_RBC or raises the existing
%      singleton*.
%
%      H = SCA_RBC returns the handle to a new SCA_RBC or the handle to
%      the existing singleton*.
%
%      SCA_RBC('Property','Value',...) creates a new SCA_RBC using the
%      given property value pairs. Unrecognized properties are passed via
%      varargin to SCA_RBC_OpeningFcn.  This calling syntax produces a
%      warning when there is an existing singleton*.
%
%      SCA_RBC('CALLBACK') and SCA_RBC('CALLBACK',hObject,...) call the
%      local function named CALLBACK in SCA_RBC.M with the given input
%      arguments.
%
%      *See GUI Options on GUIDE's Tools menu.  Choose "GUI allows only one
%      instance to run (singleton)".
%
% See also: GUIDE, GUIDATA, GUIHANDLES

% Edit the above text to modify the response to help SCA_RBC

% Last Modified by GUIDE v2.5 04-Dec-2013 09:34:20

% Begin initialization code - DO NOT EDIT

handles.path = 'rbc_1.jpg';
handles.eThreshold = 0.80 ;
handles.minimumRBC = 25;

gui_Singleton = 1;
gui_State = struct('gui_Name',       mfilename, ...
                   'gui_Singleton',  gui_Singleton, ...
                   'gui_OpeningFcn', @SCA_RBC_OpeningFcn, ...
                   'gui_OutputFcn',  @SCA_RBC_OutputFcn, ...
                   'gui_LayoutFcn',  [], ...
                   'gui_Callback',   []);
if nargin && ischar(varargin{1})
   gui_State.gui_Callback = str2func(varargin{1});
end

if nargout
    [varargout{1:nargout}] = gui_mainfcn(gui_State, varargin{:});
else
    gui_mainfcn(gui_State, varargin{:});
end
% End initialization code - DO NOT EDIT


% --- Executes just before SCA_RBC is made visible.
function SCA_RBC_OpeningFcn(hObject, eventdata, handles, varargin)
% This function has no output args, see OutputFcn.
% hObject    handle to figure
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)
% varargin   unrecognized PropertyName/PropertyValue pairs from the
%            command line (see VARARGIN)

% Choose default command line output for SCA_RBC
handles.output = hObject;

% Update handles structure
guidata(hObject, handles);

% UIWAIT makes SCA_RBC wait for user response (see UIRESUME)
% uiwait(handles.figure1);


% --- Outputs from this function are returned to the command line.
function varargout = SCA_RBC_OutputFcn(hObject, eventdata, handles)
% varargout  cell array for returning output args (see VARARGOUT);
% hObject    handle to figure
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Get default command line output from handles structure
varargout{1} = handles.output;



function edit1_Callback(hObject, eventdata, handles)
% hObject    handle to edit1 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hints: get(hObject,'String') returns contents of edit1 as text
%        str2double(get(hObject,'String')) returns contents of edit1 as a double

if isfield(handles, 'filePath')
    set(handles.edit1 , 'String' , handles.filePath );
end

guidata(hObject,handles); %save data to handles

% --- Executes during object creation, after setting all properties.
function edit1_CreateFcn(hObject, eventdata, handles)
% hObject    handle to edit1 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: edit controls usually have a white background on Windows.
%       See ISPC and COMPUTER.
if ispc && isequal(get(hObject,'BackgroundColor'), get(0,'defaultUicontrolBackgroundColor'))
    set(hObject,'BackgroundColor','white');
end


% --- Executes on button press in pushbutton1 - Run Test.
function pushbutton1_Callback(hObject, eventdata, handles)
% hObject    handle to pushbutton1 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

if isfield(handles, 'filePath')
    filesDir = handles.filePath;
else
    display('Choose file');
    return;
end

srcFiles = dir(strcat(filesDir,'\','*jpg'));  % the folder in which ur images exists

% arrays to store result of test of each image
imageFilesNames = [];
countArray = [];
totalArray = [];
testResults = [];

for i = 1 : length(srcFiles)
    
    fileName = strcat(filesDir,'\',srcFiles(i).name);
    imageFilesNames = [imageFilesNames {srcFiles(i).name}];
    
    I = imread(fileName);
    J = rgb2gray(I);
    K = imadjust(J);
    %figure, imshow(K), title('Original');

    %reversing color for better view of objects
    K = imcomplement(K);
    %figure, imshow(K);

    threshold = graythresh(K);
    bw = im2bw(K,threshold);
    %figure, imshow(bw);

    %fill the holes 
    bwFill = imfill(bw, 'holes');
    %figure, imshow(bwFill), title('binary image with filled holes');

    %remove border objects
    bwNObord = imclearborder(bwFill, 4);
    %figure, imshow(bwNObord), title('cleared border image');

    % remove all object containing fewer than rmPixs pixels
    rmPixs = 100; % depends on original image size
    bw = bwareaopen(bwNObord,rmPixs);
    %figure, imshow(bw), title('removed small objects');

    if isfield(handles,'eThreshold')
        eMin = handles.eThreshold;
    else
        eMin = .80; %default value of eThreshold
    end
    
    if isfield(handles,'minimumRBC')
        rbcMin = handles.minimumRBC;
    else
        rbcMin = 25; %default value of minimumRBC
    end

    count = 0; % Sickel Cell count

    % Find the Boundaries, B =  no of separate objects 
    [B,L] = bwboundaries(bw,'noholes');

    % Display the label matrix and draw each boundary

    %imshow(label2rgb(L, @jet, [.5 .5 .5]))
    label2rgb(L, @jet, [.5 .5 .5]);

    hold on

    stats = regionprops(L,'Eccentricity');

    for k = 1: length(stats)
    
        % obtain (X,Y) boundary coordinates corresponding to label 'k'
        boundary = B{k};
    
        e = stats(k).Eccentricity;
    
        if e > eMin
            count = count + 1;
        end
        
        %metric_string = sprintf('%2.2f',e);
        
        %text(boundary(1,2)-1,boundary(1,1)+1,metric_string,'Color','black',...
        %'FontSize',14,'FontWeight','bold');
    end

    countArray(length(countArray)+1) = count;
    totalArray(length(totalArray)+1) = length(stats);

    if count > (rbcMin*length(stats))/100
        testResults(length(testResults)+1) = 1; % 1 indicates test positive
    else
        testResults(length(testResults)+1) = 0; % 0 indicates test negative
    end

    title(['Values closer to 1 indicate ',...
       'sickel cell']);

end

stem(totalArray,countArray);
display(countArray);
display(totalArray);
display(testResults);
display(imageFilesNames);

return;


% --- Executes on selection change in popupmenu1.
function popupmenu1_Callback(hObject, eventdata, handles)
% hObject    handle to popupmenu1 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% Hints: contents = cellstr(get(hObject,'String')) returns popupmenu1 contents as cell array
%        contents{get(hObject,'Value')} returns selected item from popupmenu1


% --- Executes during object creation, after setting all properties.
function popupmenu1_CreateFcn(hObject, eventdata, handles)
% hObject    handle to popupmenu1 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: popupmenu controls usually have a white background on Windows.
%       See ISPC and COMPUTER.
if ispc && isequal(get(hObject,'BackgroundColor'), get(0,'defaultUicontrolBackgroundColor'))
    set(hObject,'BackgroundColor','white');
end


% --- Executes on selection change in popupmenu3.
function popupmenu3_Callback(hObject, eventdata, handles)

contents = get(hObject,'Value');

switch contents
    case 1
        handles.eThreshold = 0.50;
    case 2
        handles.eThreshold = 0.55;
    case 3
        handles.eThreshold = 0.60;
    case 4
        handles.eThreshold = 0.65;
    case 5
        handles.eThreshold = 0.70;
    case 6
        handles.eThreshold = 0.75;
    case 7
        handles.eThreshold = 0.80;
    case 8
        handles.eThreshold = 0.85;
    case 9
        handles.eThreshold = 0.90;
    case 10
        handles.eThreshold = 0.95;
    otherwise
        handles.eThreshold = 0.80;
end

guidata(hObject,handles); %save data to handles

% --- Executes during object creation, after setting all properties.
function popupmenu3_CreateFcn(hObject, eventdata, handles)
% hObject    handle to popupmenu3 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: popupmenu controls usually have a white background on Windows.
%       See ISPC and COMPUTER.
if ispc && isequal(get(hObject,'BackgroundColor'), get(0,'defaultUicontrolBackgroundColor'))
    set(hObject,'BackgroundColor','white');
end


% --- Executes on selection change in popupmenu4.
function popupmenu4_Callback(hObject, eventdata, handles)


contents = get(hObject,'Value');

switch contents
    case 1
        handles.minimumRBC = 10;
    case 2
        handles.minimumRBC = 15;
    case 3
        handles.minimumRBC = 20;
    case 4
        handles.minimumRBC = 25;
    case 5
        handles.minimumRBC = 30;
    case 6
        handles.minimumRBC = 35;
    case 7
        handles.minimumRBC = 40;
    case 8
        handles.minimumRBC = 45;
    case 9
        handles.minimumRBC = 50;
    case 10
        handles.minimumRBC = 55;
    case 11
        handles.minimumRBC = 60;
    case 12
        handles.minimumRBC = 65;
    case 13
        handles.minimumRBC = 70;
    otherwise
        handles.minimumRBC = 25;
end

guidata(hObject,handles); %save data to handles


% --- Executes during object creation, after setting all properties.
function popupmenu4_CreateFcn(hObject, eventdata, handles)
% hObject    handle to popupmenu4 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    empty - handles not created until after all CreateFcns called

% Hint: popupmenu controls usually have a white background on Windows.
%       See ISPC and COMPUTER.
if ispc && isequal(get(hObject,'BackgroundColor'), get(0,'defaultUicontrolBackgroundColor'))
    set(hObject,'BackgroundColor','white');
end


% --- Executes on button press in pushbutton3 - Browse.
function pushbutton3_Callback(hObject, eventdata, handles)
% hObject    handle to pushbutton3 (see GCBO)
% eventdata  reserved - to be defined in a future version of MATLAB
% handles    structure with handles and user data (see GUIDATA)

% to read perticular .jpg file 
%[FileEx,PathEx] = uigetfile('*.jpg','Select the Emission Correction File'); t
%ExPath = [PathEx FileEx];

% to read directory 
ExPath = uigetdir;
handles.filePath = ExPath;
guidata(hObject,handles); %save data to handles
edit1_Callback(hObject, eventdata, handles);
